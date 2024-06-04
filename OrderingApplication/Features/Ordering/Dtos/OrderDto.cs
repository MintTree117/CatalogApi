using OrderingDomain.Orders;
using OrderingDomain.ValueTypes;

namespace OrderingApplication.Features.Ordering.Dtos;

internal readonly record struct OrderDto(
    Address ShippingAddress,
    Address BillingAddress,
    bool SaveNewAddress,
    DateTime OrderDate,
    Pricing Pricing,
    OrderState State,
    List<OrderGroupDto> OrderGroups )
{
    internal static Order Model( OrderDto order ) => new() {
        ShippingAddress = order.ShippingAddress,
        BillingAddress = order.BillingAddress,
        OrderDate = order.OrderDate,
        Pricing = order.Pricing,
        State = OrderState.Processing
    };
    internal static Order Model( OrderDto order, Guid customerId ) => new() {
        CustomerId = customerId,
        ShippingAddress = order.ShippingAddress,
        BillingAddress = order.BillingAddress,
        OrderDate = order.OrderDate,
        Pricing = order.Pricing,
        State = OrderState.Processing
    };
};