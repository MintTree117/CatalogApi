using System.Data;
using CatalogApplication.Database;
using CatalogApplication.Types._Common.ReplyTypes;
using CatalogApplication.Types.Brands.Dtos;
using CatalogApplication.Types.Brands.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Repositories.Features;

internal sealed class BrandRepository : BaseRepository<BrandRepository>
{
    readonly MemoryCache<BrandsReply, BrandRepository> _memoryCache;

    public BrandRepository( IDapperContext dapper, ILogger<BrandRepository> logger ) : base( dapper, logger )
        => _memoryCache = new MemoryCache<BrandsReply, BrandRepository>( TimeSpan.FromHours( 1 ), FetchBrands, logger );

    internal async Task<Reply<BrandsReply>> GetBrands()
    {
        var cacheReply = await _memoryCache.Get();
        return cacheReply
            ? cacheReply
            : await FetchBrands();
    }
    async Task<Reply<BrandsReply>> FetchBrands()
    {
        const string sql =
            """
            SELECT * FROM CatalogApi.Brands;
            SELECT * FROM CatalogApi.BrandCategories;
            """;
        
        try 
        {
            await using SqlConnection connection = await Dapper.GetOpenConnection();

            if (connection.State != ConnectionState.Open) {
                LogError( $"Invalid connection state: {connection.State}" );
                return Reply<BrandsReply>.ServerError();
            }

            await using SqlMapper.GridReader reader = await connection.QueryMultipleAsync( sql, commandType: CommandType.Text );
            BrandsReply brands = new(
                (await reader.ReadAsync<Brand>()).ToList(),
                (await reader.ReadAsync<BrandCategory>()).ToList() );

            LogInformation( "Brands fetched from database." );
            return Reply<BrandsReply>.Success( brands );
        }
        catch ( Exception e ) 
        {
            LogException( e, $"Error while attempting to fetch brands from database: {e.Message}" );
            return Reply<BrandsReply>.ServerError();
        }
    }
}