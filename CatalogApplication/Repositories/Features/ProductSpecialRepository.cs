using System.Data;
using CatalogApplication.Database;
using CatalogApplication.Types._Common.ReplyTypes;
using CatalogApplication.Types.Products.Dtos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Repositories.Features;

internal sealed class ProductSpecialRepository : BaseRepository<ProductSpecialRepository>
{
    readonly MemoryCache<SpecialsDto, ProductSpecialRepository> _memoryCache;

    public ProductSpecialRepository( IDapperContext dapper, ILogger<ProductSpecialRepository> logger ) : base( dapper, logger )
        => _memoryCache = new MemoryCache<SpecialsDto, ProductSpecialRepository>( TimeSpan.FromHours( 1 ), FetchSpecials, logger );
    
    internal async Task<Reply<SpecialsDto>> GetSpecials()
    {
        var cacheReply = await _memoryCache.Get();
        return cacheReply
            ? cacheReply
            : await FetchSpecials();
    }
    async Task<Reply<SpecialsDto>> FetchSpecials()
    {
        const string sql =
            """
            SELECT * FROM CatalogApi.Products WHERE IsFeatured = 1 ORDER BY NumberSold DESC OFFSET 0 FETCH NEXT 10 ROWS ONLY;
            SELECT * FROM CatalogApi.Products WHERE SalePrice > 0 ORDER BY NumberSold DESC OFFSET 0 FETCH NEXT 10 ROWS ONLY;
            SELECT * FROM CatalogApi.Products ORDER BY NumberSold DESC OFFSET 0 FETCH NEXT 10 ROWS ONLY;
            """;

        try
        {
            await using SqlConnection connection = await Dapper.GetOpenConnection();

            if (connection.State != ConnectionState.Open)
            {
                LogError( $"Invalid connection state: {connection.State}" );
                return Reply<SpecialsDto>.ServerError();
            }

            await using SqlMapper.GridReader multi = await connection.QueryMultipleAsync( sql, commandType: CommandType.Text );
            SpecialsDto specials = new(
                (await multi.ReadAsync<ProductDto>()).ToList(),
                (await multi.ReadAsync<ProductDto>()).ToList(),
                (await multi.ReadAsync<ProductDto>()).ToList() );

            return Reply<SpecialsDto>.Success( specials );
        }
        catch ( Exception e )
        {
            LogException( e, $"An exception occured while executing get specials: {e.Message}" );
            return Reply<SpecialsDto>.ServerError();
        }
    }
}