using OrderingDomain.Billing;
using OrderingDomain.Optionals;
using OrderingDomain.Orders;

namespace OrderingApplication.Features.Billing;

internal sealed class BillingService
{
    internal async Task<Invoice> CreateInvoice() => throw new Exception();
    internal async Task<Bill> CreateBill() => throw new Exception();

    internal async Task<Reply<bool>> SendInvoice( Order order ) => throw new Exception();
    internal async Task<Reply<bool>> SendBill( Order order ) => throw new Exception();
}