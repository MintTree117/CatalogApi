using OrderingDomain.Optionals;
using OrderingDomain.Orders;

namespace OrderingInfrastructure.Features.Ordering.Repositories;

public interface IOrderingUtilityRepository
{
    Task<Reply<bool>> SaveAsync();
    Task<Reply<bool>> InsertOrderProblem( OrderProblem problem );
    Task<Reply<bool>> InsertPendingCancelLine( OrderLine line );
    Task<Reply<bool>> DeletePendingDeleteLine( OrderLine line );
    Task<Replies<OrderStateDelayTime>> GetDelayTimes();
    Task<Replies<OrderStateExpireTime>> GetExpiryTimes();
    Task<Replies<OrderLine>> GetTopUnhandledDelayedOrderLines( int amount, int checkHours );
    Task<Replies<OrderLine>> GetTopUnhandledExpiredOrderLines( int amount, int checkHours );
    Task<Replies<OrderLine>> GetPendingCancelLines();
}