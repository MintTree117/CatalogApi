using System.Data;
using CatalogApplication.Database;
using CatalogApplication.Types.Brands.Dtos;
using CatalogApplication.Types.Brands.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Repositories;

internal sealed class BrandRepository
{
    readonly IDapperContext _dapper;
    readonly ILogger<BrandRepository> _logger;
    readonly TimeSpan _cacheLifeMinutes = TimeSpan.FromMinutes( 10 );
    
    bool _isUpdating = false;
    Timer _timer;
    DateTime _lastCacheUpdate = DateTime.Now;
    BrandsReply? _filters = null;
    
    public BrandRepository( IDapperContext dapper, ILogger<BrandRepository> logger )
    {
        _dapper = dapper;
        _logger = logger;
        _timer = new Timer( _ => Update(), null, TimeSpan.Zero, _cacheLifeMinutes );
    }

    internal async Task<BrandsReply?> GetBrands()
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

        return await FetchBrands();
    }
    async Task<bool> FetchBrands()
    {
        const string sql =
            """
            SELECT * FROM CatalogApi.Brands;
            SELECT * FROM CatalogApi.BrandCategories;
            """;
        
        try {
            
            await using SqlConnection connection = await _dapper.GetOpenConnection();

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

        bool success = await FetchBrands();
        
        if (!success) _logger.LogError( "Brands Update Failed." );
        else _logger.LogInformation( "Brands Update Success." );
        
        _isUpdating = false;
    }
}