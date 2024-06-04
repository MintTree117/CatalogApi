namespace OrderingApplication.Features.Ordering.Dtos;

internal readonly record struct OrderCancelRequest(
    Guid OrderId );