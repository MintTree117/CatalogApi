namespace CatalogApplication.Types.Stock;

internal readonly record struct Warehouse(
    Guid Id,
    Guid AddressId,
    string Name );