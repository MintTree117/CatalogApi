using CatalogApplication.Database;
using CatalogApplication.Types._Common.ReplyTypes;
using CatalogApplication.Types.Categories;

namespace CatalogApplication.Repositories;

internal sealed class CategoryRepository // SINGLETON
{
    readonly IDapperContext _dapper;
    readonly ILogger<CategoryRepository> _logger;
    readonly TimeSpan _cacheLifeMinutes = TimeSpan.FromMinutes( 10 );
    readonly object _cacheLock = new();

    bool _isUpdating = false;
    Timer _timer;
    DateTime _lastCacheUpdate = DateTime.Now;
    List<Category>? _cachedCategories = null;

    public CategoryRepository( IDapperContext dapper, ILogger<CategoryRepository> logger )
    {
        _dapper = dapper;
        _logger = logger;
        _timer = new Timer( _ => Update(), null, TimeSpan.Zero, _cacheLifeMinutes );
    }

    internal async Task<IEnumerable<Category>> GetCategories()
    {
        if (_cachedCategories is not null && _cachedCategories.Count != 0 && DateTime.Now - _lastCacheUpdate < _cacheLifeMinutes)
            return _cachedCategories;

        return await FetchCategoriesWait()
            ? _cachedCategories ?? []
            : [];
    }
    async Task<bool> FetchCategoriesWait()
    {
        const int Safety = 10;
        int count = 0;
        while ( _isUpdating && count < Safety ) {
            await Task.Delay( 500 );
            count++;
        }

        return await FetchCategories();
    }
    async Task<bool> FetchCategories()
    {
        const string sql = "SELECT * FROM Categories";
        
        Replies<Category> categories = await _dapper.QueryAsync<Category>( sql );

        if (!categories.IsSuccess) {
            _logger.LogError( $"Failed to update categories cache: {categories.Message()}" );
            _cachedCategories = null;
            return false;
        }

        lock ( _cacheLock ) {
            _cachedCategories = categories.Enumerable.ToList();
            _lastCacheUpdate = DateTime.Now;
        }

        _logger.LogInformation( "Categories cache successfully updated." );
        return true;
    }
    async void Update()
    {
        _isUpdating = true;
        
        bool success = await FetchCategories();

        if (!success) _logger.LogError( "Category Update Failed." );
        else _logger.LogInformation( "Category Update Success." );

        _isUpdating = false;
    }
}