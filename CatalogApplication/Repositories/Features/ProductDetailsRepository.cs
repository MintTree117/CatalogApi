using CatalogApplication.Database;
using CatalogApplication.Types._Common.ReplyTypes;
using CatalogApplication.Types.Products.Dtos;
using Dapper;

namespace CatalogApplication.Repositories.Features;

internal sealed class ProductDetailsRepository : BaseRepository<ProductDetailsRepository>
{
    readonly TimeSpan _cacheLifeMinutes = TimeSpan.FromMinutes( 5 );
    readonly object _cacheLock = new();
    readonly Timer _timer;
    
    Dictionary<Guid, CacheEntry> _cache = [];
    
    public ProductDetailsRepository( IDapperContext dapper, ILogger<ProductDetailsRepository> logger ) : base( dapper, logger )
    {
        _timer = new Timer( _ => Cleanup(), null, TimeSpan.Zero, _cacheLifeMinutes );
    }
    internal async Task<ProductDetailsDto?> GetDetails( Guid productId )
    {
        if (_cache.TryGetValue( productId, out CacheEntry entry ) && entry.Valid())
            return entry.DetailsDto;

        var fetchReply = await FetchDetails( productId );
        return fetchReply
            ? fetchReply.Data
            : null;
    }
    
    async Task<Reply<ProductDetailsDto>> FetchDetails( Guid productId )
    {
        // language=sql
        const string sql =
            """
            SELECT
                p.Id, p.BrandId, p.IsFeatured, p.IsInStock, p.Name, p.Image, p.Price, p.SalePrice, p.Rating, p.NumberRatings,
                (SELECT CategoryId FROM CatalogApi.ProductCategories WHERE ProductId = @productId) AS CategoryId,
                pd.Description,
                px.Xml
            FROM CatalogApi.Products p
                INNER JOIN CatalogApi.ProductDescriptions pd ON p.Id = pd.ProductId
                INNER JOIN CatalogApi.ProductXmls px ON p.Id = px.ProductId
                WHERE p.Id = @productId;
            """;
        
        try 
        {
            DynamicParameters parameters = new();
            parameters.Add( "productId", productId );
            var reply = await Dapper.QueryFirstOrDefaultAsync<ProductDetailsDto>( sql, parameters );
            
            if (!reply)
                return Reply<ProductDetailsDto>.NotFound();

            ProductDetailsDto details = reply.Data;
            AddToCache( details );
            return Reply<ProductDetailsDto>.Success( details );
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