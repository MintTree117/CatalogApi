using CatalogApplication.Database;
using CatalogApplication.Types._Common.Geography;
using CatalogApplication.Types._Common.ReplyTypes;
using CatalogApplication.Types.Products.Dtos;
using CatalogApplication.Types.Stock;

namespace CatalogApplication.Repositories;

internal sealed class InventoryRepository
{
    readonly IServiceProvider _provider;
    readonly ILogger<InventoryRepository> _logger;
    readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes( 30 );
    readonly object _cacheLock = new();

    readonly Dictionary<int, int> ShippingDistanceDays = new() {
        { 1000, 1 },
        { 2000, 3 },
        { 8000, 7 },
        { 16000, 30 },
    };
    const int DefaultDays = 30;
    const int GridCellLength = 1000000;
    const int SpatialCellSize = 50000;

    Timer _cacheTimer;
    Dictionary<int, Dictionary<Cell, Dictionary<Guid, int>>> _stockByWarehouseBySpatialRegionIndex = [];

    readonly record struct StockDto(
        Guid ItemId,
        int Quantity );
    
    // CONSTRUCTOR
    public InventoryRepository( IServiceProvider provider, ILogger<InventoryRepository> logger )
    {
        _provider = provider;
        _logger = logger;
        _cacheTimer = new Timer( _ => OnCacheTimer(), null, TimeSpan.Zero, _cacheLifetime );
    }
    
    // GET ESTIMATES
    internal async Task<List<int>> GetDeliveryEstimates( List<SearchDto> items, AddressDto? deliveryAddress )
    {
        if (deliveryAddress is null) {
            List<int> def = [];
            for ( int i = 0; i < items.Count; i++ )
                def.Add( DefaultDays );
            return def;
        }
        
        try {
            return await Task.Run( () => {
                List<int> days = [];
                List<int> cells = CalculateCells( deliveryAddress.Value.PosX, deliveryAddress.Value.PosY, 16000 );
                foreach ( SearchDto dto in items )
                    days.Add( GetEstimateDays( dto.ProductId, GetCell( deliveryAddress.Value ), cells ) );
                return days;
            } );
        }
        catch ( Exception e ) {
            _logger.LogError( e, $"An error occured while executing GetDeliveryEstimateDays() : {e.Message}" );
            return [];
        }
    }
    int GetEstimateDays( Guid itemId, Cell destination, List<int> cellsToCheck )
    {
        double nearestDistance = double.MaxValue;
        foreach ( int i in cellsToCheck ) {
            var locations = _stockByWarehouseBySpatialRegionIndex[i];
            foreach ( var kvp in locations ) {
                Cell locationCell = kvp.Key;
                var entries = kvp.Value;
                if (!entries.TryGetValue( itemId, out int quantity ) || quantity <= 0)
                    continue;
                double distance = GetDistance( locationCell, destination );
                if (!(distance < nearestDistance))
                    continue;
                nearestDistance = distance;

                KeyValuePair<int, int> first = ShippingDistanceDays.First();
                if (nearestDistance < first.Key)
                    return first.Value;
            }
        }

        int days = DefaultDays;
        foreach ( var kvp in ShippingDistanceDays )
            if (nearestDistance < kvp.Key)
                days = kvp.Value;
        return days;
    }
    static int CalculateCell( int posX, int posY ) =>
        (posX / SpatialCellSize) + (posY / SpatialCellSize) * (GridCellLength / SpatialCellSize);
    static List<int> CalculateCells( int startX, int startY, int radius )
    {
        int startMetaCellX = startX / SpatialCellSize;
        int startMetaCellY = startY / SpatialCellSize;

        List<int> spatialCells = [];
        for ( int dx = -radius; dx <= radius; dx++ ) {
            for ( int dy = -radius; dy <= radius; dy++ ) {
                int metaCellX = startMetaCellX + dx;
                int metaCellY = startMetaCellY + dy;
                spatialCells.Add( CalculateCell( metaCellX, metaCellY ) );
            }
        }
        return spatialCells;
    }
    static Cell GetCell( AddressDto address ) =>
        new( address.PosX, address.PosY );
    static double GetDistance( Cell a, Cell b ) =>
        Math.Sqrt( Math.Pow( a.X - b.X, 2 ) + Math.Pow( a.Y - b.Y, 2 ) );
    
    // REFRESH CACHE
    async void OnCacheTimer()
    {
        if (await RefreshInventory())
            _logger.LogInformation( "Refreshed Inventory Repository" );
        else
            _logger.LogError( "Failed to refresh Inventory Repository" );
    }
    async Task<bool> RefreshInventory()
    {
        // GET WAREHOUSE DATA
        List<Warehouse>? warehouses = await GetWarehouseData();
        if (warehouses is null)
            return false;
        
        // Dictionary(Region,Dictionary(Location,Stock))
        Dictionary<int, Dictionary<Cell, Dictionary<Guid, int>>> newCache = [];
        using HttpClient http = GetHttpClient();
        
        // CHECK EACH WAREHOUSE...
        foreach ( Warehouse w in warehouses ) {
            Cell warehouseCell = new( w.PosX, w.PosY );
            int cell = CalculateCell( w.PosX, w.PosY );
            
            if (!newCache.TryGetValue( cell, out var locations )) {
                locations = new Dictionary<Cell, Dictionary<Guid, int>>();
                newCache.Add( cell, locations );
            }

            if (!locations.TryGetValue( warehouseCell, out var stock )) {
                stock = new Dictionary<Guid, int>();
                locations.Add( warehouseCell, stock );
            }
            
            // PAGINATE THE ITEM STOCK INFO AS IT MAY BE HUGE
            const int pageSize = 100;
            int page = 1;
            bool hasMoreData;
            do {
                string url = $"{w.QueryUrl}?page={page}&pageSize={pageSize}";
                List<StockDto>? queryResult = await http.GetFromJsonAsync<List<StockDto>>( url );
                
                if (queryResult is null || queryResult.Count == 0)
                    break;

                foreach ( StockDto s in queryResult )
                    stock.TryAdd( s.ItemId, s.Quantity );

                hasMoreData = queryResult.Count == pageSize;
                page++;
            } while ( hasMoreData );
        }
        
        // SET CACHE
        lock ( _cacheLock )
            _stockByWarehouseBySpatialRegionIndex = newCache;
        return true;
    }
    async Task<List<Warehouse>?> GetWarehouseData()
    {
        const string sql = "SELECT * FROM Warehouses";
        IDapperContext context = IDapperContext.GetContext( _provider );
        Replies<Warehouse> result = await context.QueryAsync<Warehouse>( sql );
        return result.IsSuccess
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
}