namespace CatalogApplication.Types.Orders;

internal readonly record struct OrderItemDto(
    Guid ProductId,
    int Quantity );