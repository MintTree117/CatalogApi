namespace CatalogApplication.Types.Warehouses;

internal sealed record Warehouse(
    Guid Id,
    string QueryUrl,
    int PosX,
    int PosY );