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
    readonly MemoryCache<BrandsDto, BrandRepository> _memoryCache;

    public BrandRepository( IDapperContext dapper, ILogger<BrandRepository> logger ) : base( dapper, logger )
        => _memoryCache = new MemoryCache<BrandsDto, BrandRepository>( TimeSpan.FromHours( 1 ), FetchBrands, logger );

    internal async Task<Reply<BrandsDto>> GetBrands()
    {
        var cacheReply = await _memoryCache.Get();
        return cacheReply
            ? cacheReply
            : await FetchBrands();
    }
    async Task<Reply<BrandsDto>> FetchBrands()
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
                return Reply<BrandsDto>.ServerError();
            }

            await using SqlMapper.GridReader reader = await connection.QueryMultipleAsync( sql, commandType: CommandType.Text );
            
            var brands = await reader.ReadAsync<Brand>();
            var brandCategories = await reader.ReadAsync<BrandCategory>();

            Dictionary<Guid, HashSet<Guid>> brandCategoriesDictionary = [];
            foreach ( BrandCategory b in brandCategories )
            {
                if (!brandCategoriesDictionary.TryGetValue( b.CategoryId, out HashSet<Guid>? brandIds ))
                {
                    brandIds = [];
                    brandCategoriesDictionary.Add( b.CategoryId, brandIds );
                }
                brandIds.Add( b.BrandId );
            }
            
            LogInformation( "Brands fetched from database." );
            return Reply<BrandsDto>.Success( 
                new BrandsDto( brands.ToList(), brandCategoriesDictionary ) );
        }
        catch ( Exception e ) 
        {
            LogException( e, $"Error while attempting to fetch brands from database: {e.Message}" );
            return Reply<BrandsDto>.ServerError();
        }
    }
}