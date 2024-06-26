using CatalogApplication.Database;
using CatalogApplication.Types._Common.ReplyTypes;
using CatalogApplication.Types.Categories;

namespace CatalogApplication.Repositories.Features;

internal sealed class CategoryRepository : BaseRepository<CategoryRepository>
{
    readonly MemoryCache<List<Category>, CategoryRepository> _memoryCache;
    
    public CategoryRepository( IDapperContext dapper, ILogger<CategoryRepository> logger ) : base( dapper, logger )
        => _memoryCache = new MemoryCache<List<Category>, CategoryRepository>( TimeSpan.FromHours( 1 ), FetchCategories, logger );
    
    internal async Task<Reply<List<Category>>> GetCategories()
    {
        var cacheReply = await _memoryCache.Get();
        return cacheReply
            ? cacheReply
            : await FetchCategories();
    }
    async Task<Reply<List<Category>>> FetchCategories()
    {
        const string sql = "SELECT * FROM CatalogApi.Categories";

        var reply = await Dapper.QueryAsync<Category>( sql );
        if (!reply) {
            LogError( $"Failed to fetch categories from database: {reply.GetMessage()}" );
            return Reply<List<Category>>.ServerError();
        }

        LogInformation( "Categories fetched from database." );
        return Reply<List<Category>>.Success( reply.Enumerable.ToList() );
    }
}