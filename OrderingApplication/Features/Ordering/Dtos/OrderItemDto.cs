using OrderingDomain.Orders;

namespace OrderingApplication.Features.Ordering.Dtos;

internal readonly record struct OrderItemDto(
    Guid OrderGroupId,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    OrderState State )
{
    internal static List<OrderItem> Models( IEnumerable<OrderItemDto> dtos )
        => dtos.Select( MapToModel ).ToList();
    static OrderItem MapToModel( OrderItemDto item )
        => new( Guid.Empty, Guid.Empty, Guid.Empty, item.ProductId, item.ProductName, item.Quantity, item.Price, item.State );
}