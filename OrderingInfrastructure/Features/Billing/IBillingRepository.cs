using OrderingDomain.Billing;
using OrderingDomain.Optionals;

namespace OrderingInfrastructure.Features.Billing;

public interface IBillingRepository
{
    Task<Reply<Invoice>> GetInvoice( Guid orderId );
    Task<Reply<Bill>> GetBill( Guid orderId );

    Task<Reply<bool>> AddInvoice( Invoice invoice );
    Task<Reply<bool>> AddBill( Bill bill );
}