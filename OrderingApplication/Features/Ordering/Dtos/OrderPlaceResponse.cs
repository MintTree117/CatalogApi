namespace OrderingApplication.Features.Ordering.Dtos;

internal readonly record struct OrderPlaceResponse(
    Guid OrderId,
    bool DidProcess,
    List<Guid> UnavailableItems )
{
    public static OrderPlaceResponse Unavailable( List<Guid> unavailableItems ) => new( Guid.Empty, false, unavailableItems );
    public static OrderPlaceResponse Placed( Guid orderId ) => new( orderId, true, [] );
}