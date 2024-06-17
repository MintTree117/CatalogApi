using CatalogApplication.Database;
using CatalogApplication.Types._Common.Geography;
using CatalogApplication.Types._Common.ReplyTypes;
using CatalogApplication.Types.Search.Dtos;
using CatalogApplication.Types.Stock;

namespace CatalogApplication.Repositories;

internal sealed class InventoryRepository
{
    readonly IServiceProvider _provider;
    readonly IDapperContext _dapper;
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
    
    // In-Memory Cache
    Dictionary<int,
        Dictionary<Cell,
            Dictionary<Guid, int>>> _stockByWarehouseBySpatialRegionIndex = [];

    readonly record struct StockDto(
        Guid ItemId,
        int Quantity );
    
    // CONSTRUCTOR
    public InventoryRepository( IServiceProvider provider, IDapperContext dapper, ILogger<InventoryRepository> logger )
    {
        _provider = provider;
        _dapper = dapper;
        _logger = logger;
        _cacheTimer = new Timer( _ => OnCacheTimer(), null, TimeSpan.Zero, _cacheLifetime );

        async void OnCacheTimer()
        {
            if (await RefreshInventory())
                _logger.LogInformation( "Refreshed Inventory Repository" );
            else
                _logger.LogError( "Failed to refresh Inventory Repository" );
        }
    }
    
    // GET ESTIMATES
    internal async Task<List<int>> GetDeliveryEstimates( List<SearchItemDto> items, AddressDto? deliveryAddress )
    {
        if (deliveryAddress is null)
            return GetDefaultDays( items.Count );
        
        try
        {
            return await CalculateEstimates( items, deliveryAddress.Value );
        }
        catch ( Exception e ) {
            _logger.LogError( e, $"An error occured while executing GetDeliveryEstimateDays() : {e.Message}" );
            return [];
        }
    }
    async Task<List<int>> CalculateEstimates( List<SearchItemDto> items, AddressDto deliveryAddress )
    {
        return await Task.Run( () => {
            List<int> estimates = [];
            List<int> cellsToCheck = CalculateCellsToCheck( deliveryAddress.PosX, deliveryAddress.PosY, 16000 );
            foreach ( SearchItemDto dto in items )
            {
                var cell = new Cell( deliveryAddress.PosX, deliveryAddress.PosY );
                var estimate = GetEstimateDays( dto.ProductId, cell, cellsToCheck );
                estimates.Add( estimate );
            }
            return estimates;
        } );
    }
    int GetEstimateDays( Guid itemId, Cell destination, List<int> cellsToCheck )
    {
        double nearestDistance = double.MaxValue;
        foreach ( int cell in cellsToCheck ) {
            var locationsToCheck = _stockByWarehouseBySpatialRegionIndex[cell];
            foreach ( var kvp in locationsToCheck ) {
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
    static List<int> CalculateCellsToCheck( int startX, int startY, int radius )
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
    static int CalculateCell( int posX, int posY ) =>
        (posX / SpatialCellSize) + (posY / SpatialCellSize) * (GridCellLength / SpatialCellSize);
    static double GetDistance( Cell a, Cell b ) =>
        Math.Sqrt( Math.Pow( a.X - b.X, 2 ) + Math.Pow( a.Y - b.Y, 2 ) );
    static List<int> GetDefaultDays( int count )
    {
        List<int> defaults = [];
        for ( int i = 0; i < count; i++ )
            defaults.Add( DefaultDays );
        return defaults;
    }
    
    // REFRESH CACHE
    async Task<bool> RefreshInventory()
    {
        // GET WAREHOUSE DATA
        var warehouses = await GetWarehouseData();
        if (warehouses is null)
            return false;
        
        // Dictionary(Region,Dictionary(Location,Stock))
        Dictionary<int, Dictionary<Cell, Dictionary<Guid, int>>> newCache = [];
        using HttpClient http = GetHttpClient();
        
        // CHECK EACH WAREHOUSE...
        foreach ( Warehouse w in warehouses ) 
        {
            Cell warehouseCell = new( w.PosX, w.PosY );
            int cell = CalculateCell( w.PosX, w.PosY );
            
            if (!newCache.TryGetValue( cell, out var locations )) 
            {
                locations = new Dictionary<Cell, Dictionary<Guid, int>>();
                newCache.Add( cell, locations );
            }
            if (!locations.TryGetValue( warehouseCell, out var warehouseInventory )) 
            {
                warehouseInventory = new Dictionary<Guid, int>();
                locations.Add( warehouseCell, warehouseInventory );
            }
            
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
                    warehouseInventory.TryAdd( s.ItemId, s.Quantity );

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
        var result = await _dapper.QueryAsync<Warehouse>( sql );
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
}