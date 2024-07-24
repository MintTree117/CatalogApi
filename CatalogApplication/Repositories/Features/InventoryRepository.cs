using CatalogApplication.Database;
using CatalogApplication.Types._Common.Geography;
using CatalogApplication.Types._Common.ReplyTypes;
using CatalogApplication.Types.Orders;
using CatalogApplication.Types.Products.Models;
using CatalogApplication.Types.Warehouses;

namespace CatalogApplication.Repositories.Features;

internal sealed class InventoryRepository : BaseRepository<InventoryRepository>
{
    const int DefaultDays = 30;
    const int MaxWarehouseCheckRadius = 16000;
    readonly Dictionary<int, int> ShippingDistanceDays = new() {
        { 1000, 1 },
        { 2000, 3 },
        { 8000, 7 },
        { 16000, 30 },
    };   
    readonly MemoryCache<WarehouseCache, InventoryRepository> _warehouses;
    readonly MemoryCache<InventoryCache, InventoryRepository> _inventories;

    // CONSTRUCTOR
    public InventoryRepository( IDapperContext dapper, ILogger<InventoryRepository> logger ) : base( dapper, logger )
    {
        _warehouses = new MemoryCache<WarehouseCache, InventoryRepository>( TimeSpan.FromHours( 4 ), RefreshWarehouses, logger );
        _inventories = new MemoryCache<InventoryCache, InventoryRepository>( TimeSpan.FromMinutes( 15 ), RefreshInventories, logger );
    }
    
    // VALIDATE ORDER
    internal async Task<Reply<List<OrderCatalogItemDto>>> ValidateOrder( CatalogOrderDto order )
    {
        var warehousesTask = GetWarehouses();
        var inventoriesTask = GetInventories();

        await Task.WhenAll( warehousesTask, inventoriesTask );
        
        if (warehousesTask.Result is null || inventoriesTask.Result is null)
        {
            Logger.LogWarning( $"Null result in ValidateOrder(): WarehouseTaskResult {warehousesTask.Result} : InventoriesTaskResult {inventoriesTask.Result}" );
            return Reply<List<OrderCatalogItemDto>>.NotFound();
        }
        
        List<OrderCatalogItemDto> catalogItems = [];
        bool isValidOrder = true;
        
        foreach ( CartItemDto dto in order.Items )
        {
            Guid? warehouseId = GetOrderItemWarehouseId(
                dto, new AddressDto( order.PosX, order.PosY ), warehousesTask.Result, inventoriesTask.Result );
            
            if (warehouseId is null)
            {
                isValidOrder = false; // terminate the order   
                break;
            }

            OrderCatalogItemDto catalogItem = new() {
                WarehouseId = warehouseId.Value,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };
            catalogItems.Add( catalogItem );
        }

        return isValidOrder
            ? Reply<List<OrderCatalogItemDto>>.Success( catalogItems )
            : Reply<List<OrderCatalogItemDto>>.Conflict( "Insufficient stock for order." );
    }
    static Guid? GetOrderItemWarehouseId( CartItemDto item, AddressDto address, List<Warehouse> warehouses, Dictionary<Warehouse, Dictionary<Guid, int>> inventories )
    {
        Guid? nearestId = null;
        double nearestDistance = double.MaxValue;
        foreach ( Warehouse w in warehouses )
        {
            if (!inventories.TryGetValue( w, out var inventory ))
                continue;

            if (!inventory.TryGetValue( item.ProductId, out int quantity ) || quantity < item.Quantity)
                continue;
            
            double distance = CalculateDistance( address.PosX, address.PosY, w.PosX, w.PosY );
            if (!(distance < nearestDistance))
                continue;
            
            nearestDistance = distance;
            nearestId = w.Id;
        }

        return nearestId;
    }
    
    // SHIPPING ESTIMATES
    internal async Task<List<int>> GetShippingEstimates( List<Guid> itemIds, AddressDto? deliveryAddress )
    {
        if (deliveryAddress is null)
            return GetDefaultDays( itemIds.Count );

        var warehouses = await GetWarehouses();
        if (warehouses is null)
            return GetDefaultDays( itemIds.Count );
        
        try
        {
            return await CalculateShippingEstimates( itemIds, deliveryAddress.Value, warehouses );
        }
        catch ( Exception e ) {
            LogException( e, "An error occured while executing GetDeliveryEstimateDays()." );
            return GetDefaultDays( itemIds.Count );
        }
    }
    async Task<List<int>> CalculateShippingEstimates( List<Guid> itemIds, AddressDto address, List<Warehouse> warehouses )
    {
        var inventories = await GetInventories();
        if (inventories is null)
            return GetDefaultDays( itemIds.Count );

        return await Task.Run( () => {
            List<int> estimates = [];
            List<Warehouse> warehousesToCheck = [];

            foreach ( Warehouse w in warehouses )
                if (CalculateDistance( address.PosX, address.PosY, w.PosX, w.PosY ) < MaxWarehouseCheckRadius)
                    warehousesToCheck.Add( w );

            foreach ( Guid i in itemIds )
                estimates.Add( GetEstimateDays( i, address, warehousesToCheck, inventories ) );

            return estimates;
        } );
    }
    int GetEstimateDays( Guid itemId, AddressDto address, List<Warehouse> warehouses, Dictionary<Warehouse, Dictionary<Guid, int>> inventories )
    {
        double nearestDistance = double.MaxValue;
        foreach ( Warehouse w in warehouses )
        {
            if (!inventories.TryGetValue( w, out var inventory ))
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
    async Task<Reply<WarehouseCache>> RefreshWarehouses()
    {
        var reply = await FetchWarehouses();
        return reply
            ? Reply<WarehouseCache>.Success( WarehouseCache.From( reply.Data ) )
            : Reply<WarehouseCache>.Failure();
    }
    async Task<Reply<InventoryCache>> RefreshInventories()
    {
        var reply = await FetchInventories();
        return reply
            ? Reply<InventoryCache>.Success( InventoryCache.From( reply.Data ) )
            : Reply<InventoryCache>.Failure();
    }
    
    // FETCH
    async Task<Reply<List<Warehouse>>> FetchWarehouses()
    {
        const string sql = "SELECT * FROM CatalogApi.Warehouses";
        var replies = await Dapper.QueryAsync<Warehouse>( sql );

        return replies
            ? Reply<List<Warehouse>>.Success( replies.Enumerable.ToList() )
            : Reply<List<Warehouse>>.ServerError();
    }
    async Task<Reply<Dictionary<Warehouse, Dictionary<Guid, int>>>> FetchInventories()
    {
        var warehouseReply = await _warehouses.Get();
        if (!warehouseReply)
            return Reply<Dictionary<Warehouse, Dictionary<Guid, int>>>.Failure();

        var warehouses = warehouseReply.Data.Warehouses;
        Dictionary<Warehouse, Dictionary<Guid, int>> newCache = [];
        foreach ( Warehouse w in warehouses )
            newCache.TryAdd( w, [] );
        
        const string sql = "SELECT * FROM CatalogApi.ProductInventories";
        var invReplies = await Dapper.QueryAsync<ProductInventory>( sql );
        if (!invReplies)
            return Reply<Dictionary<Warehouse, Dictionary<Guid, int>>>.ServerError();
        
        foreach ( var inv in invReplies.Enumerable )
        {
            var warehouse = warehouses.FirstOrDefault( w => w.Id == inv.WarehouseId );
            if (warehouse is null)
                continue;

            if (!newCache.TryGetValue( warehouse, out Dictionary<Guid, int>? inventory ))
            {
                inventory = [];
                newCache.Add( warehouse, inventory );
            }
            
            inventory.Add( inv.ProductId, inv.Quantity );
        }

        return Reply<Dictionary<Warehouse, Dictionary<Guid, int>>>.Success( newCache );
    }
    
    // GET CACHE HELPERS
    async Task<List<Warehouse>?> GetWarehouses()
    {
        List<Warehouse>? warehouses = null;

        var warehouseCacheReply = await _warehouses.Get();
        if (warehouseCacheReply)
            return warehouseCacheReply.Data.Warehouses;
        var fetchReply = await FetchWarehouses();
        if (fetchReply)
            warehouses = fetchReply.Data;

        return warehouses;
    }
    async Task<Dictionary<Warehouse, Dictionary<Guid, int>>?> GetInventories()
    {
        Dictionary<Warehouse, Dictionary<Guid, int>>? inventories = null;

        var inventoriesCacheReply = await _inventories.Get();
        if (inventoriesCacheReply) 
            return inventoriesCacheReply.Data.Inventories;
        var fetchReply = await FetchInventories();
        if (fetchReply)
            inventories = fetchReply.Data;

        return inventories;
    }
    
    // LOCAL HELPER TYPES
    sealed class WarehouseCache
    {
        public List<Warehouse> Warehouses = [];
        public static WarehouseCache From( List<Warehouse> data ) =>
            new() { Warehouses = data };
    }
    sealed class InventoryCache
    {
        public Dictionary<Warehouse, Dictionary<Guid, int>> Inventories = [];
        public static InventoryCache From( Dictionary<Warehouse, Dictionary<Guid, int>> data ) =>
            new() { Inventories = data };
    }
}