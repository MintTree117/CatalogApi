using OrderingDomain._Common;
using OrderingDomain.ValueTypes;

namespace OrderingDomain.Billing;

public sealed class Bill : IEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid InvoiceId { get; set; } = Guid.Empty;
    public Guid OrderId { get; set; } = Guid.Empty;
    public DateTime BillingDate { get; set; } = DateTime.Now;
    public DateTime InvoiceDate { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime FulfillmentDate { get; set; }
    public DateTime DueDate { get; set; }
    public Pricing Pricing { get; set; }

    public static Bill FromInvoice( Invoice invoice, DateTime fulfillmentDate ) =>
        new()
        {
            InvoiceId = invoice.Id,
            OrderId = invoice.OrderId,
            BillingDate = DateTime.Now,
            InvoiceDate = invoice.InvoiceDate,
            OrderDate = invoice.OrderDate,
            FulfillmentDate = fulfillmentDate,
            Pricing = invoice.Pricing
        };
}