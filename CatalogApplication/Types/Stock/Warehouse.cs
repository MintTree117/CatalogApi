namespace CatalogApplication.Types.Stock;

internal record Warehouse(
    Guid Id,
    string QueryUrl,
    int PosX,
    int PosY );