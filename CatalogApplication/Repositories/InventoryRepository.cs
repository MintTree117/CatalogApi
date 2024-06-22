using CatalogApplication.Database;
using CatalogApplication.Types._Common.Geography;
using CatalogApplication.Types.Products.Models;
using CatalogApplication.Types.Stock;

namespace CatalogApplication.Repositories;

internal sealed class InventoryRepository : BaseRepository<InventoryRepository>
{
    readonly IServiceProvider _provider;
    readonly IDapperContext _dapper;
    readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes( 30 );
    readonly object _cacheLock = new();

    readonly Dictionary<int, int> ShippingDistanceDays = new() {
        { 1000, 1 },
        { 2000, 3 },
        { 8000, 7 },
        { 16000, 30 },
    };
    const int DefaultDays = 30;
    const int MaxWarehouseCheckRadius = 16000;

    Timer _cacheTimer;
    List<Warehouse> _warehouses = [];
    Dictionary<Warehouse, Dictionary<Guid, int>> _inventories = [];
    
    // CONSTRUCTOR
    public InventoryRepository( IServiceProvider provider, IDapperContext dapper, ILogger<InventoryRepository> logger ) : base( logger )
    {
        _provider = provider;
        _dapper = dapper;
        _cacheTimer = new Timer( _ => OnCacheTimer(), null, TimeSpan.FromMinutes( 5 ), _cacheLifetime );

        async void OnCacheTimer()
        {
            if (await RefreshInventory())
                LogInformation( "Refreshed inventory repository." );
            else
                LogError( "Failed to refresh inventory repository." );
        }
    }
    
    // GET ESTIMATES
    internal async Task<List<int>> GetDeliveryEstimates( List<Guid> itemIds, AddressDto? deliveryAddress )
    {
        if (deliveryAddress is null)
            return GetDefaultDays( itemIds.Count );

        if (_warehouses.Count <= 0)
        {
            LogError( "no count" );
            await RefreshInventory();
        }
        
        try
        {
            return await CalculateEstimates( itemIds, deliveryAddress.Value );
        }
        catch ( Exception e ) {
            LogException( e, "An error occured while executing GetDeliveryEstimateDays()." );
            return GetDefaultDays( itemIds.Count );
        }
    }
    async Task<List<int>> CalculateEstimates( List<Guid> itemIds, AddressDto deliveryAddress )
    {
        return await Task.Run( () => {
            List<int> estimates = [];
            List<Warehouse> warehousesToCheck = [];

            foreach ( Warehouse w in _warehouses )
                if (CalculateDistance( deliveryAddress.PosX, deliveryAddress.PosY, w.PosX, w.PosY ) < MaxWarehouseCheckRadius)
                    warehousesToCheck.Add( w );
            
            foreach ( Guid i in itemIds )
                estimates.Add( GetEstimateDays( i, deliveryAddress, warehousesToCheck ) );
            
            return estimates;
        } );
    }
    int GetEstimateDays( Guid itemId, AddressDto address, List<Warehouse> warehouses )
    {
        double nearestDistance = double.MaxValue;
        foreach ( Warehouse w in warehouses )
        {
            if (!_inventories.TryGetValue( w, out var inventory ))
                continue;

            if (!inventory.TryGetValue( itemId, out int quantity ) || quantity <= 0)
                continue;

            double distance = CalculateDistance( address.PosX, address.PosY, w.PosX, w.PosY );
            if (!(distance < nearestDistance))
                continue;
            nearestDistance = distance;
        }
        
        int days = DefaultDays;
        foreach ( var kvp in ShippingDistanceDays.Reverse() )
            if (nearestDistance < kvp.Key)
                days = kvp.Value;
        return days;
    }
    static List<int> GetDefaultDays( int count )
    {
        List<int> defaults = [];
        for ( int i = 0; i < count; i++ )
            defaults.Add( DefaultDays );
        return defaults;
    }
    static double CalculateDistance( int x1, int y1, int x2, int y2 )
    {
        return Math.Sqrt( Math.Pow( x2 - x1, 2 ) + Math.Pow( y2 - y1, 2 ) );
    }
    
    // REFRESH CACHE
    async Task<bool> RefreshInventory()
    {
        // GET WAREHOUSE DATA
        var warehouses = await GetWarehouseData();
        if (warehouses is null)
            return false;
        
        Dictionary<Warehouse, Dictionary<Guid, int>> newCache = [];
        foreach ( Warehouse w in warehouses )
            newCache.TryAdd( w, [] );


        var inventories = await GetInventoryData();
        if (inventories is null)
            return false;
        
        foreach ( var inv in inventories )
        {
            Warehouse? warehouse = warehouses.FirstOrDefault( w => w.Id == inv.WarehouseId );
            if (warehouse is null)
                continue;

            if (!newCache.TryGetValue( warehouse, out Dictionary<Guid, int>? inventory ))
            {
                inventory = [];
                newCache.Add( warehouse, inventory );
            }
            inventory.Add( inv.ProductId, inv.Quantity );
        }
        
        // SET CACHE
        lock ( _cacheLock )
        {
            _warehouses = warehouses;
            _inventories = newCache;
        }
        return true;
    }
    async Task<List<Warehouse>?> GetWarehouseData()
    {
        const string sql = "SELECT * FROM CatalogApi.Warehouses";
        var result = await _dapper.QueryAsync<Warehouse>( sql );
        return result
            ? result.Enumerable.ToList()
            : null;
    }
    async Task<List<ProductInventory>?> GetInventoryData()
    {
        const string sql = "SELECT * FROM CatalogApi.ProductInventories";
        var result = await _dapper.QueryAsync<ProductInventory>( sql );
        return result
            ? result.Enumerable.ToList()
            : null;
    }
    HttpClient GetHttpClient()
    {
        using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return (scope.ServiceProvider.GetService<IHttpClientFactory>() 
            ?? throw new Exception( "Failed to create HttpClient." ))
            .CreateClient() 
            ?? throw new Exception( "Failed to create HttpClient." );
    }
    readonly record struct StockDto(
        Guid ItemId,
        int Quantity );
}

/*using HttpClient http = GetHttpClient();

// CHECK EACH WAREHOUSE...
foreach ( Warehouse w in warehouses )
{
    Dictionary<Guid, int> inv = [];
    // PAGINATE THE ITEM STOCK INFO AS IT MAY BE HUGE
    const int pageSize = 100;
    int page = 1;
    bool hasMoreData;
    do
    {
        string url = $"{w.QueryUrl}?page={page}&pageSize={pageSize}";
        List<StockDto>? queryResult = await http.GetFromJsonAsync<List<StockDto>>( url );

        if (queryResult is null || queryResult.Count == 0)
            break;

        foreach ( StockDto s in queryResult )
            inv.TryAdd( s.ItemId, s.Quantity );

        hasMoreData = queryResult.Count == pageSize;
        page++;
    } while ( hasMoreData );

    newCache.TryAdd( w, inv );
}*/