using OrderingDomain.Orders;

namespace OrderingApplication.Features.Ordering.Dtos;

internal readonly record struct OrderGroupDto(
    Guid Id,
    Guid OrderId,
    List<OrderItemDto> Items,
    OrderState State );