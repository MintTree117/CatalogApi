namespace OrderingApplication.Features.Ordering.Dtos;

internal readonly record struct OrderPlaceRequest(
    Guid UserId,
    OrderDto Order,
    List<OrderItemDto> Items );