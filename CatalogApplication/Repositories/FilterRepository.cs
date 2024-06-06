using System.Data;
using CatalogApplication.Database;
using CatalogApplication.Types.Filters.Dtos;
using CatalogApplication.Types.Filters.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Repositories;

internal sealed class FilterRepository
{
    readonly IServiceProvider _provider;
    readonly ILogger<FilterRepository> _logger;
    readonly TimeSpan _cacheLifeMinutes = TimeSpan.FromMinutes( 10 );
    
    bool _isUpdating = false;
    Timer _timer;
    DateTime _lastCacheUpdate = DateTime.Now;
    FiltersReply? _filters = null;

    public FilterRepository( IServiceProvider provider, ILogger<FilterRepository> logger )
    {
        _provider = provider;
        _logger = logger;
        _timer = new Timer( _ => Update(), null, TimeSpan.Zero, _cacheLifeMinutes );
    }

    internal async Task<FiltersReply?> GetFilters()
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
            SELECT * FROM PriceRanges;
            SELECT * FROM RatingLevels;
            SELECT * FROM ShippingTimespans;
            """;
        
        try {
            
            DapperContext dapper = GetContext();
            await using SqlConnection connection = await dapper.GetOpenConnection();

            if (connection.State != ConnectionState.Open) {
                _logger.LogError( $"Invalid connection state: {connection.State}" );
                return false;
            }

            await using SqlMapper.GridReader reader = await connection.QueryMultipleAsync( sql, commandType: CommandType.Text );
            FiltersReply filters = new(
                (await reader.ReadAsync<Brand>()).ToList(),
                (await reader.ReadAsync<BrandCategory>()).ToList(),
                (await reader.ReadAsync<PriceRange>()).ToList(),
                (await reader.ReadAsync<RatingLevel>()).ToList(),
                (await reader.ReadAsync<ShippingTimespan>()).ToList() );

            _filters = filters;
            _lastCacheUpdate = DateTime.Now;
            _logger.LogInformation( "Filter Repository Updated." );
            return true;
        }
        catch ( Exception e ) {
            _logger.LogError( e, $"Error while attempting to fetch filters from repository: {e.Message}" );
            return false;
        }
    }
    async void Update()
    {
        _isUpdating = true;

        bool success = await FetchFilters();
        
        if (!success) _logger.LogError( "Filter Update Failed." );
        else _logger.LogInformation( "Filter Update Success." );
        
        _isUpdating = false;
    }
    DapperContext GetContext()
    {
        using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return scope.ServiceProvider.GetService<DapperContext>() ?? throw new Exception( "Failed to get DapperContext" );
    }
}