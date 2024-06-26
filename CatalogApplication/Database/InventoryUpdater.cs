namespace CatalogApplication.Database;

public class InventoryUpdater
{
    /*HttpClient GetHttpClient()
    {
        using AsyncServiceScope scope = _provider.CreateAsyncScope();
        return (scope.ServiceProvider.GetService<IHttpClientFactory>()
                ?? throw new Exception( "Failed to create HttpClient." ))
            .CreateClient()
            ?? throw new Exception( "Failed to create HttpClient." );
    }*/

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
}