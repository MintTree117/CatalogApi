namespace CatalogApplication.Types.Stock;

internal readonly record struct Inventory(
    Guid Id,
    Guid ProductId,
    Guid WarehouseId,
    int Quantity );