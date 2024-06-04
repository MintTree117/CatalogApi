using OrderingDomain._Common;

namespace OrderingDomain.Orders;

public sealed class OrderItem : IEntity
{
    public OrderItem() { }

    public OrderItem(
        Guid id,
        Guid orderId,
        Guid orderLineId,
        Guid productId,
        string productName,
        int quantity,
        decimal price,
        OrderState state )
    {
        Id = id;
        OrderId = orderId;
        OrderLineId = orderLineId;
        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        Price = price;
        State = state;
    }

    public Guid Id { get; set; } = Guid.Empty;
    public Guid OrderId { get; set; } = Guid.Empty;
    public Guid OrderLineId { get; set; } = Guid.Empty;
    public Guid ProductId { get; set; } = Guid.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public OrderState State { get; set; } = OrderState.Processing;
}