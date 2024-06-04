using OrderingDomain._Common;

namespace OrderingDomain.Orders;

public sealed class OrderLine : IEntity
{
    public OrderLine() { }
    public OrderLine( Guid orderId, Guid warehouseId )
    {
        OrderId = orderId;
        WarehouseId = warehouseId;
        State = OrderState.Processing;
    }

    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid WarehouseId { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.Now;
    public DateTime LastUpdate { get; set; } = DateTime.Now;
    public OrderState State { get; set; } = OrderState.Processing;
    public bool Delayed { get; set; }
    public bool Problem { get; set; }
}