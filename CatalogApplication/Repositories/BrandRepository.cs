using System.Data;
using CatalogApplication.Database;
using CatalogApplication.Types.Brands.Dtos;
using CatalogApplication.Types.Brands.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Repositories;

internal sealed class BrandRepository : BaseRepository<BrandRepository>
{
    readonly IDapperContext _dapper;
    readonly TimeSpan _cacheLifeMinutes = TimeSpan.FromMinutes( 10 );
    readonly object _cacheLock = new();
    
    bool _isUpdating = false;
    Timer _timer;
    DateTime _lastCacheUpdate = DateTime.Now;
    BrandsReply? _filters = null;
    
    public BrandRepository( IDapperContext dapper, ILogger<BrandRepository> logger ) : base(logger)
    {
        _dapper = dapper;
        _timer = new Timer( _ => Update(), null, TimeSpan.Zero, _cacheLifeMinutes );

        async void Update()
        {
            bool success = await FetchBrands();

            if (!success) LogError( "Brands update failed." );
            else LogInformation( "Brands update success." );
        }
    }

    internal async Task<BrandsReply?> GetBrands()
    {
        const int Safety = 5;
        int count = 0;
        bool waited = false;
        while ( _isUpdating && count < Safety )
        {
            waited = true;
            await Task.Delay( 500 );
            count++;
        }

        bool filtersValid =
            _filters is not null &&
            DateTime.Now - _lastCacheUpdate < _cacheLifeMinutes;

        if (filtersValid || waited)
            return _filters;
        
        await FetchBrands();
        return _filters;
    }
    async Task<bool> FetchBrands()
    {
        const string sql =
            """
            SELECT * FROM CatalogApi.Brands;
            SELECT * FROM CatalogApi.BrandCategories;
            """;
        
        try 
        {
            lock ( _cacheLock )
                _isUpdating = true;
            
            await using SqlConnection connection = await _dapper.GetOpenConnection();

            if (connection.State != ConnectionState.Open) {
                LogError( $"Invalid connection state: {connection.State}" );
                return false;
            }

            await using SqlMapper.GridReader reader = await connection.QueryMultipleAsync( sql, commandType: CommandType.Text );
            BrandsReply brands = new(
                (await reader.ReadAsync<Brand>()).ToList(),
                (await reader.ReadAsync<BrandCategory>()).ToList() );

            lock ( _cacheLock )
            {
                _filters = brands;
                _lastCacheUpdate = DateTime.Now;
                _isUpdating = false;
            }

            LogInformation( "Brands updated." );
            return true;
        }
        catch ( Exception e ) 
        {
            LogException( e, $"Error while attempting to fetch brands from repository: {e.Message}" );
            return false;
        }
    }
}