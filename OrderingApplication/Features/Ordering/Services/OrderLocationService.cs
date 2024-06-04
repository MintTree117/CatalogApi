using OrderingDomain.Optionals;
using OrderingDomain.Orders;
using OrderingInfrastructure.Http;

namespace OrderingApplication.Features.Ordering.Services;

internal sealed class OrderLocationService( OrderLocationCache cache, IHttpService http )
{
    internal readonly OrderLocationCache Cache = cache;
    readonly IHttpService _http = http;

    string BaseUrl( Guid locationId ) => Cache.LocationsById[locationId].ApiUrl;
    string CheckUrl( Guid locationId ) => $"{BaseUrl( locationId )}/check";
    string PlaceUrl( Guid locationId ) => $"{BaseUrl( locationId )}/place";
    string CancelUrl( Guid locationId ) => $"{BaseUrl( locationId )}/cancel";

    internal async Task<Reply<bool>> CheckOrderStock( Guid locationId, Guid itemId, int itemQuantity ) =>
        await _http.TryGetObjRequest<bool>(
            CheckUrl( locationId ),
            IHttpService.QueryParameters( nameof( itemId ), nameof( itemQuantity ), itemId, itemQuantity ) );
    internal async Task<Reply<bool>> PlaceOrderLine( OrderLine orderLine ) =>
        await _http.TryPostObjRequest<bool>(
            PlaceUrl( orderLine.WarehouseId ),
            orderLine );
    internal async Task<Reply<bool>> StartCancelOrderLine( OrderLine orderLine ) =>
        await _http.TryPostObjRequest<bool>(
            CancelUrl( orderLine.WarehouseId ),
            orderLine );
    internal async Task<Reply<bool>> RevertCancelOrderLine( OrderLine orderLine ) =>
        await _http.TryPostObjRequest<bool>(
            CancelUrl( orderLine.WarehouseId ),
            orderLine );
    internal async Task<Reply<bool>> ConfirmCancelOrderLine( OrderLine orderLine ) =>
        await _http.TryPostObjRequest<bool>(
            CancelUrl( orderLine.WarehouseId ),
            orderLine );
}