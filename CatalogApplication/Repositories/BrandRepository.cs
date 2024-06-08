using System.Data;
using CatalogApplication.Database;
using CatalogApplication.Seeding;
using CatalogApplication.Types.Brands.Dtos;
using CatalogApplication.Types.Brands.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Repositories;

internal sealed class BrandRepository
{
    readonly IServiceProvider _provider;
    readonly ILogger<BrandRepository> _logger;
    readonly TimeSpan _cacheLifeMinutes = TimeSpan.FromMinutes( 10 );
    
    bool _isUpdating = false;
    Timer _timer;
    DateTime _lastCacheUpdate = DateTime.Now;
    BrandsReply? _filters = null;
    
    public BrandRepository( IServiceProvider provider, ILogger<BrandRepository> logger )
    {
        _provider = provider;
        _logger = logger;
        _timer = new Timer( _ => Update(), null, TimeSpan.Zero, _cacheLifeMinutes );
    }

    internal async Task<BrandsReply?> GetFilters()
    {
        if (_filters is not null && DateTime.Now - _lastCacheUpdate < _cacheLifeMinutes)
            return _filters;

        return await FetchFiltersWait()
            ? _filters
            : null;
    }
    async Task<bool> FetchFiltersWait()
    {
        const int Safety = 10;
        int count = 0;
        while ( _isUpdating && count < Safety ) {
            await Task.Delay( 500 );
            count++;
        }

        return await FetchFilters();
    }
    async Task<bool> FetchFilters()
    {
        const string sql =
            """
            SELECT * FROM Brands;
            SELECT * FROM BrandCategories;
            """;
        
        try {

            IDapperContext dapper = IDapperContext.GetContext( _provider );
            await using SqlConnection connection = await dapper.GetOpenConnection();

            if (connection.State != ConnectionState.Open) {
                _logger.LogError( $"Invalid connection state: {connection.State}" );
                return false;
            }

            await using SqlMapper.GridReader reader = await connection.QueryMultipleAsync( sql, commandType: CommandType.Text );
            BrandsReply brands = new(
                (await reader.ReadAsync<Brand>()).ToList(),
                (await reader.ReadAsync<BrandCategory>()).ToList() );

            _filters = brands;
            _lastCacheUpdate = DateTime.Now;
            _logger.LogInformation( "Brand Repository Updated." );
            return true;
        }
        catch ( Exception e ) {
            _logger.LogError( e, $"Error while attempting to fetch brands from repository: {e.Message}" );
            return false;
        }
    }
    async void Update()
    {
        _isUpdating = true;

        bool success = await FetchFilters();
        
        if (!success) _logger.LogError( "Brands Update Failed." );
        else _logger.LogInformation( "Brands Update Success." );
        
        _isUpdating = false;
    }
}