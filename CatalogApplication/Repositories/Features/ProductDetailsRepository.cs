using CatalogApplication.Database;
using CatalogApplication.Types._Common.ReplyTypes;
using CatalogApplication.Types.Products.Dtos;
using Dapper;

namespace CatalogApplication.Repositories.Features;

internal sealed class ProductDetailsRepository : BaseRepository<ProductDetailsRepository>
{
    readonly TimeSpan _cacheLifeMinutes = TimeSpan.FromMinutes( 5 );
    readonly object _cacheLock = new();
    // ReSharper disable once NotAccessedField.Local
    readonly Timer _timer;
    
    Dictionary<Guid, CacheEntry> _cache = [];
    
    public ProductDetailsRepository( IDapperContext dapper, ILogger<ProductDetailsRepository> logger ) : base( dapper, logger )
    {
        _timer = new Timer( _ => Cleanup(), null, TimeSpan.Zero, _cacheLifeMinutes );
    }
    internal async Task<Reply<ProductDetailsDto>> GetDetails( Guid productId )
    {
        if (_cache.TryGetValue( productId, out CacheEntry entry ) && entry.Valid())
            return Reply<ProductDetailsDto>.Success( entry.DetailsDto );

        var fetchReply = await FetchDetails( productId );
        if (fetchReply)
            AddToCache( fetchReply.Data );
        
        return fetchReply
            ? Reply<ProductDetailsDto>.Success( fetchReply.Data )
            : Reply<ProductDetailsDto>.Failure( fetchReply.GetMessage() );
    }
    async Task<Reply<ProductDetailsDto>> FetchDetails( Guid productId )
    {
        const string sql =
            """
                SELECT
                p.Id, p.BrandId, p.Name, p.BrandName, p.Image, p.IsFeatured, p.IsInStock, p.Price, p.SalePrice, p.SaleEndDate, p.ReleaseDate, p.Rating, p.NumberRatings,
                c.CategoryId,
                pd.Description,
                px.Xml
                FROM CatalogApi.Products p
                INNER JOIN CatalogApi.ProductCategories c ON p.Id = c.ProductId
                INNER JOIN CatalogApi.ProductDescriptions pd ON p.Id = pd.ProductId
                INNER JOIN CatalogApi.ProductXmls px ON p.Id = px.ProductId
                WHERE p.Id = @productId;
            """;

        try
        {
            await using var connection = await Dapper.GetOpenConnection();
            var productDictionary = new Dictionary<Guid, ProductDetailsDto?>();
            await connection.QueryAsync<ProductDetailsDto, Guid, string, string, ProductDetailsDto>(
                sql,
                ( product, categoryId, description, xml ) => {
                    if (!productDictionary.TryGetValue( product.Id, out ProductDetailsDto? currentProduct ))
                    {
                        currentProduct = product;
                        currentProduct.CategoryIds = new List<Guid>();
                        productDictionary.Add( currentProduct.Id, currentProduct );
                    }
                    currentProduct?.CategoryIds?.Add( categoryId );
                    if (!string.IsNullOrWhiteSpace( description ) && currentProduct is not null )
                        currentProduct.Description = description;
                    if (!string.IsNullOrWhiteSpace( xml ) && currentProduct is not null )
                        currentProduct.Xml = xml;
                    return product;
                },
                new { productId },
                splitOn: "CategoryId,Description,Xml"
            );

            var dto = productDictionary.Values.FirstOrDefault();
            return dto is not null
                ? Reply<ProductDetailsDto>.Success( dto )
                : Reply<ProductDetailsDto>.NotFound();
        }
        catch ( Exception e )
        {
            LogException( e, $"Error while attempting to fetch product details from repository: {e.Message}" );
            return Reply<ProductDetailsDto>.ServerError();
        }
    }
    void Cleanup()
    {
        lock ( _cacheLock ) {
            List<Guid> toRemove = [];
            foreach ( Guid id in _cache.Keys ) {
                bool condition =
                    _cache.TryGetValue( id, out CacheEntry entry ) &&
                    DateTime.Now - entry.Expiry > _cacheLifeMinutes;
                if (!condition)
                    toRemove.Add( id );
            }

            foreach ( Guid id in toRemove )
                _cache.Remove( id );

            Dictionary<Guid, CacheEntry> newCache = [];
            foreach ( KeyValuePair<Guid, CacheEntry> kvp in _cache )
                newCache.TryAdd( kvp.Key, kvp.Value );
            _cache = newCache;
        }
    }
    void AddToCache( ProductDetailsDto dto )
    {
        CacheEntry entry = CacheEntry.ExpiresIn( dto, _cacheLifeMinutes );
        _cache.TryAdd( dto.Id, entry );
    }

    readonly record struct CacheEntry(
        ProductDetailsDto DetailsDto,
        DateTime Expiry )
    {
        internal static CacheEntry ExpiresIn( ProductDetailsDto dto, TimeSpan timeSpan ) =>
            new( dto, DateTime.Now + timeSpan );
        internal bool Valid() =>
            DateTime.Now < Expiry;
    }
}