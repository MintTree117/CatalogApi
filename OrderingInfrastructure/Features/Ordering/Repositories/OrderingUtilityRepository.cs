using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderingDomain.Optionals;
using OrderingDomain.Orders;

namespace OrderingInfrastructure.Features.Ordering.Repositories;

internal sealed class OrderingUtilityRepository( OrderingDbContext db, ILogger<OrderingUtilityRepository> logger ) : InfrastructureService<OrderingUtilityRepository>( logger ), IOrderingUtilityRepository
{
    readonly OrderingDbContext db = db;

    public async Task<Reply<bool>> SaveAsync()
    {
        try {
            return await db.SaveChangesAsync() > 0
                ? Reply<bool>.With( true )
                : Reply<bool>.None( DbExceptionMessage );
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> InsertOrderProblem( OrderProblem problem )
    {
        try {
            await db.OrderProblems.AddAsync( problem );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> InsertPendingCancelLine( OrderLine line )
    {
        try {
            await db.PendingCancelOrderLines.AddAsync( line );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> DeletePendingDeleteLine( OrderLine line )
    {
        try {
            db.PendingCancelOrderLines.Remove( line );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Replies<OrderStateDelayTime>> GetDelayTimes()
    {
        try {
            return Replies<OrderStateDelayTime>.With(
                await db.DelayTimes.ToListAsync() );
        }
        catch ( Exception e ) {
            return HandleDbExceptionOpts<OrderStateDelayTime>( e );
        }
    }
    public async Task<Replies<OrderStateExpireTime>> GetExpiryTimes()
    {
        try {
            return Replies<OrderStateExpireTime>.With(
                await db.ExpireTimes.ToListAsync() );
        }
        catch ( Exception e ) {
            return HandleDbExceptionOpts<OrderStateExpireTime>( e );
        }
    }
    public async Task<Replies<OrderLine>> GetTopUnhandledDelayedOrderLines( int amount, int checkHours )
    {
        try {
            return Replies<OrderLine>.With(
                await db.ActiveOrderLines.Where(
                    o => !o.Delayed && DateTime.Now - o.LastUpdate > TimeSpan.FromHours( checkHours ) ).ToListAsync() );
        }
        catch ( Exception e ) {
            return HandleDbExceptionOpts<OrderLine>( e );
        }
    }
    public async Task<Replies<OrderLine>> GetTopUnhandledExpiredOrderLines( int amount, int checkHours )
    {
        try {
            return Replies<OrderLine>.With(
                await db.ActiveOrderLines.Where(
                    o => !o.Problem && DateTime.Now - o.LastUpdate > TimeSpan.FromHours( checkHours ) ).ToListAsync() );
        }
        catch ( Exception e ) {
            return HandleDbExceptionOpts<OrderLine>( e );
        }
    }
    public async Task<Replies<OrderLine>> GetPendingCancelLines()
    {
        try {
            return Replies<OrderLine>.With(
                await db.ActiveOrderLines.Where( o => o.Problem ).ToListAsync() );
        }
        catch ( Exception e ) {
            return HandleDbExceptionOpts<OrderLine>( e );
        }
    }
}