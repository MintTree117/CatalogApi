using OrderingDomain._Common;

namespace OrderingDomain.Orders;

public class OrderStateDelayTime : IEntity
{
    public Guid Id { get; set; }
    public OrderState State { get; set; }
    public TimeSpan DelayTime { get; set; }
}