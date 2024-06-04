using System.Text;
using OrderingApplication.Features.Billing;
using OrderingApplication.Features.Ordering.Dtos;
using OrderingDomain.Optionals;
using OrderingDomain.Orders;
using OrderingInfrastructure.Email;
using OrderingInfrastructure.Features.Ordering.Repositories;

namespace OrderingApplication.Features.Ordering.Systems;

internal sealed class OrderUpdatingSystem( IOrderingRepository repository, BillingService billingService, IEmailSender emailSender )
{
    readonly IOrderingRepository _repository = repository;
    readonly BillingService _billingService = billingService;
    readonly IEmailSender _emailSender = emailSender;

    internal async Task<Reply<bool>> UpdateOrder( OrderUpdateRequest update )
    {
        if (update.OrderState is OrderState.Processing)
            return Reply<bool>.None( "Cannot update order status to Received." );

        if ((await _repository.GetOrderById( update.OrderId ))
            .Fails( out Reply<Order> orderResult ))
            return Reply<bool>.None( orderResult );

        if ((await _repository.GetOrderLinesByOrderId( update.OrderId ))
            .Fail( out Replies<OrderLine> orderLinesResult ))
            return Reply<bool>.None( orderLinesResult );

        if (GetCurrentOrderLine( orderLinesResult.Enumerable, update.OrderId )
            .Fails( out Reply<OrderLine> currentLineResult ))
            return Reply<bool>.None( currentLineResult );

        if (orderLinesResult.Enumerable.Any( s => s.State != currentLineResult.Data.State ))
            return await HandleLineUpdate( currentLineResult.Data, orderResult.Data.CustomerEmail );

        orderResult.Data.State = currentLineResult.Data.State;

        if ((await _repository.GetItemsForOrderLines( orderLinesResult.Enumerable ))
            .Fails( out Reply<Dictionary<OrderLine, IEnumerable<OrderItem>>> orderItemsResult ))
            return Reply<bool>.None( orderItemsResult );

        return await HandleOrderUpdate( orderResult.Data, orderLinesResult.Enumerable, orderItemsResult.Data );
    }

    async Task<Reply<bool>> HandleOrderUpdate( Order order, IEnumerable<OrderLine> lines, Dictionary<OrderLine, IEnumerable<OrderItem>> items )
    {
        return order.State switch {
            OrderState.Shipping => await HandleOrderShipped( order, lines, items ),
            OrderState.Delivered => await HandleOrderDelivered( order, lines, items ),
            _ => Reply<bool>.With( true )
        };
    }
    async Task<Reply<bool>> HandleOrderShipped( Order order, IEnumerable<OrderLine> lines, Dictionary<OrderLine, IEnumerable<OrderItem>> items )
    {
        if (_emailSender
            .SendBasicEmail( order.CustomerEmail, "Order Shipped", GenerateOrderEmailBody( order, lines, items ) )
            .Fails( out Reply<bool> emailResult ))
            return emailResult;
        return await _billingService.SendInvoice( order );
    }
    async Task<Reply<bool>> HandleOrderDelivered( Order order, IEnumerable<OrderLine> lines, Dictionary<OrderLine, IEnumerable<OrderItem>> items )
    {
        if (_emailSender
            .SendBasicEmail( order.CustomerEmail, "Order Delivered", GenerateOrderEmailBody( order, lines, items ) )
            .Fails( out Reply<bool> emailResult ))
            return emailResult;
        return await _billingService.SendBill( order );
    }

    async Task<Reply<bool>> HandleLineUpdate( OrderLine line, string email )
    {
        line.Delayed = false;
        return line.State switch {
            OrderState.Processed => await HandleLineProcessed( email, line ),
            OrderState.Fulfilling => await HandleLineFulfilling( email, line ),
            OrderState.Shipping => await HandleLineShipped( email, line ),
            OrderState.Delivered => await HandleLineDelivered( email, line ),
            OrderState.Suspended => await HandleLineSuspended( email, line ),
            OrderState.Processing => throw new ArgumentOutOfRangeException(),
            OrderState.Cancelled => throw new ArgumentOutOfRangeException(),
            OrderState.Returned => throw new ArgumentOutOfRangeException(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    async Task<Reply<bool>> HandleLineProcessed( string email, OrderLine line ) =>
        (await _repository.GetItemsForLineById( line.OrderId, line.Id ))
        .Succeeds( out Replies<OrderItem> itemsResult )
            ? _emailSender.SendBasicEmail( email, "Order Processed", GenerateLineEmailBody( line, itemsResult.Enumerable ) )
            : Reply<bool>.None( itemsResult );
    async Task<Reply<bool>> HandleLineFulfilling( string email, OrderLine line ) =>
        (await _repository.GetItemsForLineById( line.OrderId, line.Id ))
        .Succeeds( out Replies<OrderItem> itemsResult )
            ? _emailSender.SendBasicEmail( email, "Order Line Fulfilling", GenerateLineEmailBody( line, itemsResult.Enumerable ) )
            : Reply<bool>.None( itemsResult );
    async Task<Reply<bool>> HandleLineShipped( string email, OrderLine line ) =>
        (await _repository.GetItemsForLineById( line.OrderId, line.Id ))
        .Succeeds( out Replies<OrderItem> itemsResult )
            ? _emailSender.SendBasicEmail( email, "Order Line Shipped", GenerateLineEmailBody( line, itemsResult.Enumerable ) )
            : Reply<bool>.None( itemsResult );
    async Task<Reply<bool>> HandleLineDelivered( string email, OrderLine line ) =>
        (await _repository.GetItemsForLineById( line.OrderId, line.Id ))
        .Succeeds( out Replies<OrderItem> itemsResult )
            ? _emailSender.SendBasicEmail( email, "Order Line Delivered", GenerateLineEmailBody( line, itemsResult.Enumerable ) )
            : Reply<bool>.None( itemsResult );
    async Task<Reply<bool>> HandleLineSuspended( string email, OrderLine line ) =>
        (await _repository.GetItemsForLineById( line.OrderId, line.Id ))
        .Succeeds( out Replies<OrderItem> itemsResult )
            ? _emailSender.SendBasicEmail( email, "Order Line Suspended", GenerateLineEmailBody( line, itemsResult.Enumerable ) )
            : Reply<bool>.None( itemsResult );

    static Reply<OrderLine> GetCurrentOrderLine( IEnumerable<OrderLine> lines, Guid orderId )
    {
        OrderLine? currentLine = lines.FirstOrDefault( l => l.Id == orderId );
        return currentLine is null
            ? Reply<OrderLine>.None( $"Order line for order id {orderId} and order line id {orderId} not found." )
            : Reply<OrderLine>.With( currentLine );
    }
    static string GenerateOrderEmailBody( Order order, IEnumerable<OrderLine> lines, IReadOnlyDictionary<OrderLine, IEnumerable<OrderItem>> items )
    {
        StringBuilder builder = new();
        builder.Append( "<html><body>" );
        builder.Append( $"<h1>Order {order.Id} Is {order.State}</h1>" );
        foreach ( OrderLine l in lines )
            builder.Append( GenerateLineEmailBody( l, items[l] ) );
        builder.Append( "</body></html>" );
        return builder.ToString();
    }
    static string GenerateLineEmailBody( OrderLine line, IEnumerable<OrderItem> items )
    {
        StringBuilder builder = new();
        builder.Append( "<html><body>" );
        builder.Append( $"<h2>Order Line {line.Id} of Order {line.OrderId} Is {line.State}</h2>" );
        builder.Append( GenerateLineItemsTable( line, items ) );
        builder.Append( "</body></html>" );
        return builder.ToString();
    }
    static string GenerateLineItemsTable( OrderLine line, IEnumerable<OrderItem> items )
    {
        StringBuilder lineBuilder = new();
        lineBuilder.Append( $"<h2>Order Line {line.Id} of Order {line.OrderId} Fulfillment</h2>" );
        lineBuilder.Append( "<table border='1'><tr><th>Product</th><th>Quantity</th><th>Price</th></tr>" );

        foreach ( OrderItem item in items ) {
            lineBuilder.Append( "<tr>" );
            lineBuilder.Append( $"<td>{item.ProductName}</td>" );
            lineBuilder.Append( $"<td>{item.Quantity}</td>" );
            lineBuilder.Append( $"<td>{item.Price}</td>" );
            lineBuilder.Append( "</tr>" );
        }

        lineBuilder.Append( "</table>" );
        return lineBuilder.ToString();
    }
}