using OrderingDomain._Common;

namespace OrderingDomain.Orders;

public sealed class OrderStateExpireTime : IEntity
{
    public Guid Id { get; set; }
    public OrderState State { get; set; }
    public TimeSpan ExpiryTime { get; set; }
}