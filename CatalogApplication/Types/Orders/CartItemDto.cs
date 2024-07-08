namespace CatalogApplication.Types.Orders;

internal readonly record struct CartItemDto(
    Guid ProductId,
    int Quantity );