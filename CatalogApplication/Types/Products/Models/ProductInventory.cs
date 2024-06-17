namespace CatalogApplication.Types.Products.Models;

internal record ProductInventory(
    Guid ProductId,
    Guid WarehouseId,
    int Quantity );