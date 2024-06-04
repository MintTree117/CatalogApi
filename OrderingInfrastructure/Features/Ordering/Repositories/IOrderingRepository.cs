using OrderingDomain.Optionals;
using OrderingDomain.Orders;

namespace OrderingInfrastructure.Features.Ordering.Repositories;

public interface IOrderingRepository
{
    Task<Reply<bool>> SaveAsync();
    Task<Reply<bool>> InsertOrder( Order order );
    Task<Reply<bool>> InsertOrderLines( IEnumerable<OrderLine> orderLines );
    Task<Reply<bool>> InsertOrderItems( IEnumerable<OrderItem> orderItems );
    Task<Reply<bool>> DeleteOrderData( Guid orderId );
    Task<Reply<Order>> GetOrderById( Guid orderId );
    Task<Replies<OrderLine>> GetOrderLinesByOrderId( Guid orderId );
    Task<Replies<OrderItem>> GetItemsForLineById( Guid orderId, Guid orderLineId );
    Task<Reply<Dictionary<OrderLine, IEnumerable<OrderItem>>>> GetItemsForOrderLines( IEnumerable<OrderLine> lines );
}