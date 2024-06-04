using OrderingDomain._Common;

namespace OrderingDomain.Orders;

public sealed class OrderProblem : IEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid OrderLineId { get; set; }
    public OrderProblemType Type { get; set; }
}