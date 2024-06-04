using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using OrderingDomain.Optionals;
using OrderingDomain.Orders;

namespace OrderingInfrastructure.Features.Ordering.Repositories;

internal sealed class OrderingRepository( OrderingDbContext database, ILogger<OrderingRepository> logger ) : InfrastructureService<OrderingRepository>( logger ), IOrderingRepository
{
    readonly OrderingDbContext db = database;

    public async Task<Reply<bool>> SaveAsync()
    {
        try {
            return await db.SaveChangesAsync() > 0
                ? Reply<bool>.With( true )
                : Reply<bool>.None( DbNotSavedMessage );
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> InsertOrder( Order order )
    {
        try {
            await db.ActiveOrders.AddAsync( order );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> InsertOrderLines( IEnumerable<OrderLine> orderLines )
    {
        try {
            await db.AddRangeAsync( orderLines );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> InsertOrderItems( IEnumerable<OrderItem> orderItems )
    {
        try {
            await db.ActiveOrderItems.AddRangeAsync( orderItems );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> DeleteOrderData( Guid orderId )
    {
        try {
            List<OrderItem> orderItems = await db.ActiveOrderItems.Where( i => i.OrderId == orderId ).ToListAsync();
            List<OrderLine> orderLines = await db.ActiveOrderLines.Where( l => l.OrderId == orderId ).ToListAsync();
            Order? order = await db.ActiveOrders.FirstOrDefaultAsync( o => o.Id == orderId );

            db.ActiveOrderItems.RemoveRange( orderItems );
            db.ActiveOrderLines.RemoveRange( orderLines );
            if (order is not null)
                db.ActiveOrders.Remove( order );

            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<Order>> GetOrderById( Guid orderId )
    {
        try {
            Order? order = await db.ActiveOrders.FirstOrDefaultAsync( o => o.Id == orderId );
            return order is not null
                ? Reply<Order>.With( order )
                : Reply<Order>.None( $"Order {orderId} not found in db." );
        }
        catch ( Exception e ) {
            return HandleDbException<Order>( e );
        }
    }
    public async Task<Replies<OrderLine>> GetOrderLinesByOrderId( Guid orderId )
    {
        try {
            return Replies<OrderLine>.With(
                await db.ActiveOrderLines.Where( l => l.OrderId == orderId ).ToListAsync() );
        }
        catch ( Exception e ) {
            return HandleDbExceptionOpts<OrderLine>( e );
        }
    }
    public async Task<Replies<OrderItem>> GetItemsForLineById( Guid orderId, Guid orderLineId )
    {
        try {
            return Replies<OrderItem>.With(
                await db.ActiveOrderItems.Where( i => i.OrderId == orderId && i.OrderLineId == orderLineId ).ToListAsync() );
        }
        catch ( Exception e ) {
            return HandleDbExceptionOpts<OrderItem>( e );
        }
    }
    public async Task<Reply<Dictionary<OrderLine, IEnumerable<OrderItem>>>> GetItemsForOrderLines( IEnumerable<OrderLine> lines )
    {
        try {
            Dictionary<OrderLine, IEnumerable<OrderItem>> items = [];

            foreach ( OrderLine l in lines )
                if ((await GetItemsForLineById( l.OrderId, l.Id )).Fail( out Replies<OrderItem> itemsResult ))
                    return Reply<Dictionary<OrderLine, IEnumerable<OrderItem>>>.None( itemsResult );
                else
                    items.TryAdd( l, itemsResult.Enumerable );

            return Reply<Dictionary<OrderLine, IEnumerable<OrderItem>>>.With( items );
        }
        catch ( Exception e ) {
            return HandleDbException<Dictionary<OrderLine, IEnumerable<OrderItem>>>( e );
        }
    }
}