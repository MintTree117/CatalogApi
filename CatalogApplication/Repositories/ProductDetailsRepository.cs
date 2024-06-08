using System.Data;
using CatalogApplication.Database;
using CatalogApplication.Types.Products.Dtos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Repositories;

internal sealed class ProductDetailsRepository
{
    readonly IDapperContext _dapper;
    readonly ILogger<ProductDetailsRepository> _logger;
    readonly TimeSpan _cacheLifeMinutes = TimeSpan.FromMinutes( 5 );
    readonly object _cacheLock = new();
    
    Timer _timer;
    Dictionary<Guid, CacheEntry> _cache = [];

    // language=sql
    const string pname = "productId";
    // language=sql
    const string sql =
        """
        SELECT Id FROM Categories
                  WHERE ProductId = {productId};
        SELECT
            p.Id, p.BrandId, p.PriceRangeId, p.RatingLevelId, p.ShippingTimespan, p.Name, p.Image, p.Rating, p.Price, p.SalePrice,
            pd.Description,
            px.Xml
        FROM Products p
            INNER JOIN ProductDescriptions pd ON p.Id = pd.ProductId
            INNER JOIN ProductXml px ON p.Id = px.ProductId
            WHERE pd.Id = {productId};
        """;
    
    public ProductDetailsRepository( IDapperContext dapper, ILogger<ProductDetailsRepository> logger )
    {
        _dapper = dapper;
        _logger = logger;
        _timer = new Timer( _ => Cleanup(), null, TimeSpan.Zero, _cacheLifeMinutes );
    }

    internal async Task<DetailsDto?> GetDetails( Guid productId )
    {
        if (_cache.TryGetValue( productId, out CacheEntry entry ) && DateTime.Now - entry.Timestamp < _cacheLifeMinutes)
            return entry.Dto;
        
        return await FetchDetails( productId )
            ? _cache[productId].Dto
            : null;
    }
    async Task<bool> FetchDetails( Guid productId )
    {
        try {
            await using SqlConnection connection = await _dapper.GetOpenConnection();

            if (connection.State != ConnectionState.Open) {
                _logger.LogError( $"Invalid connection state: {connection.State}" );
                return false;
            }

            DynamicParameters p = new();
            p.Add( pname, productId );
            await using SqlMapper.GridReader reader = await connection.QueryMultipleAsync( sql, p, commandType: CommandType.Text );
            IEnumerable<Guid> categories = await reader.ReadAsync<Guid>();
            var details = await reader.ReadSingleAsync<DetailsDto>();
            DetailsDto result = details with { CategoryIds = categories.ToList() };
            CacheEntry entry = new( result, DateTime.Now );
            _cache.TryAdd( result.ProductId, entry );
            return true;
        }
        catch ( Exception e ) {
            _logger.LogError( e, $"Error while attempting to fetch product details from repository: {e.Message}" );
            return false;
        }
    }
    void Cleanup()
    {
        lock ( _cacheLock ) {
            List<Guid> toRemove = [];
            foreach ( Guid id in _cache.Keys ) {
                bool condition =
                    _cache.TryGetValue( id, out CacheEntry entry ) &&
                    DateTime.Now - entry.Timestamp > _cacheLifeMinutes;
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
    
    readonly record struct CacheEntry(
        DetailsDto Dto,
        DateTime Timestamp );
}