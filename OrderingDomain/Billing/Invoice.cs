using OrderingDomain._Common;
using OrderingDomain.Orders;
using OrderingDomain.ValueTypes;

namespace OrderingDomain.Billing;

public sealed class Invoice : IEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid OrderId { get; set; } = Guid.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.Now;
    public DateTime OrderDate { get; set; }
    public Pricing Pricing { get; set; }

    public static Invoice FromOrder( Order order ) =>
        new() {
            OrderId = order.Id,
            InvoiceDate = DateTime.Now,
            OrderDate = order.OrderDate,
            Pricing = order.Pricing
        };
}