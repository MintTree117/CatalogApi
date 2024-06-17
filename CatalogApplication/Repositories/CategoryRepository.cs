using CatalogApplication.Database;
using CatalogApplication.Types.Categories;

namespace CatalogApplication.Repositories;

internal sealed class CategoryRepository : BaseRepository<CategoryRepository>
{
    readonly IDapperContext _dapper;
    readonly ILogger<CategoryRepository> _logger;
    readonly TimeSpan _cacheLifeMinutes = TimeSpan.FromMinutes( 10 );
    readonly object _cacheLock = new();

    bool _isUpdating = false;
    Timer _timer;
    DateTime _lastCacheUpdate = DateTime.Now;
    List<Category>? _cachedCategories = null;

    public CategoryRepository( IDapperContext dapper, ILogger<CategoryRepository> logger ) : base(logger)
    {
        _dapper = dapper;
        _logger = logger;
        _timer = new Timer( _ => Update(), null, TimeSpan.Zero, _cacheLifeMinutes );

        async void Update()
        {
            bool success = await FetchCategories();

            if (!success) LogError( "Category Update Failed." );
            else LogInformation( "Category Update Success." );
        }
    }

    internal async Task<IEnumerable<Category>> GetCategories()
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
        
        bool validCache =
            _cachedCategories is not null &&
            _cachedCategories.Count != 0 &&
            DateTime.Now - _lastCacheUpdate < _cacheLifeMinutes;

        if (validCache || waited)
            return _cachedCategories ?? [];

        await FetchCategories();
        return _cachedCategories ?? [];
    }
    async Task<bool> FetchCategories()
    {
        const string sql = "SELECT * FROM CatalogApi.Categories";

        lock ( _cacheLock )
            _isUpdating = true;
        
        var reply = await _dapper.QueryAsync<Category>( sql );
        if (!reply) {
            LogError( $"Failed to update categories cache: {reply.GetMessage()}" );
            lock ( _cacheLock )
            {
                _cachedCategories = null;
                _isUpdating = false;
            }

            return false;
        }

        lock ( _cacheLock ) {
            _cachedCategories = reply.Enumerable.ToList();
            _lastCacheUpdate = DateTime.Now;
            _isUpdating = false;
        }

        LogInformation( "Categories cache successfully updated." );
        return true;
    }
}