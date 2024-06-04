using OrderingDomain._Common;
using OrderingDomain.ValueTypes;

namespace OrderingDomain.Orders;

public sealed class OrderLocation : IEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Address Address { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
}