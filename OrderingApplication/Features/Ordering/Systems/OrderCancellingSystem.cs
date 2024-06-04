using OrderingApplication.Features.Ordering.Dtos;
using OrderingApplication.Features.Ordering.Services;
using OrderingDomain.Optionals;
using OrderingDomain.Orders;
using OrderingInfrastructure.Features.Ordering.Repositories;

namespace OrderingApplication.Features.Ordering.Systems;

internal sealed class OrderCancellingSystem( IOrderingRepository repository, OrderLocationService locationService )
{
    readonly IOrderingRepository _repository = repository;
    readonly OrderLocationService _locationService = locationService;

    internal async Task<Reply<bool>> CancelOrder( OrderCancelRequest cancel )
    {
        if ((await _repository.GetOrderById( cancel.OrderId ))
            .Fails( out Reply<Order> orderResult ))
            return Reply<bool>.None( orderResult );

        if ((await _repository.GetOrderLinesByOrderId( cancel.OrderId ))
            .Fail( out Replies<OrderLine> orderLineResult ))
            return Reply<bool>.None( orderLineResult );

        if (!QuickCheckLines( orderLineResult.Enumerable ))
            return Reply<bool>.None( $"Order {cancel.OrderId} cannot be cancelled, if has already started fulfilling." );

        if ((await StartLineCancellations( orderLineResult.Enumerable ))
            .Fails( out Reply<bool> startCancelResult ))
            return await RevertLineCancellations( orderLineResult.Enumerable, startCancelResult );

        if ((await UpdateDatabase( orderResult.Data, orderLineResult.Enumerable ))
            .Fails( out Reply<bool> saveResult ))
            return await RevertLineCancellations( orderLineResult.Enumerable, saveResult );

        return (await ConfirmLineCancellations( orderLineResult.Enumerable ))
            .Fails( out Reply<bool> confirmResult )
                ? Reply<bool>.None( $"Failed to confirm order {cancel.OrderId} cancellation with warehouses. Please contact support. {confirmResult.Message()}" )
                : Reply<bool>.With( true );
    }

    static bool QuickCheckLines( IEnumerable<OrderLine> lines ) =>
        lines.All( l => l.State is OrderState.Processing && l.State is OrderState.Processed );
    async Task<Reply<bool>> StartLineCancellations( IEnumerable<OrderLine> lines )
    {
        foreach ( OrderLine l in lines )
            if ((await _locationService.StartCancelOrderLine( l ))
                .Fails( out Reply<bool> cancelResult ))
                return cancelResult;
        return Reply<bool>.With( true );
    }
    async Task<Reply<bool>> RevertLineCancellations( IEnumerable<OrderLine> lines, IReply result )
    {
        foreach ( OrderLine l in lines )
            if ((await _locationService.RevertCancelOrderLine( l ))
                .Fails( out Reply<bool> cancelResult ))
                return cancelResult;
        return Reply<bool>.With( true );
    }
    async Task<Reply<bool>> ConfirmLineCancellations( IEnumerable<OrderLine> lines )
    {
        foreach ( OrderLine l in lines )
            if ((await _locationService.ConfirmCancelOrderLine( l ))
                .Fails( out Reply<bool> cancelResult ))
                return cancelResult;
        return Reply<bool>.With( true );
    }
    async Task<Reply<bool>> UpdateDatabase( Order order, IEnumerable<OrderLine> lines )
    {
        order.State = OrderState.Cancelled;
        foreach ( OrderLine l in lines )
            l.State = OrderState.Cancelled;
        return await _repository.SaveAsync();
    }
}