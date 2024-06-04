using OrderingDomain.Optionals;
using OrderingDomain.Orders;
using OrderingInfrastructure.Http;

namespace OrderingApplication.Features.Ordering.Services;

internal sealed class OrderLocationCache
{
    public OrderLocationCache( IConfiguration configuration, IHttpService httpService )
    {
        http = httpService;
        TimeSpan refreshIntervalHours = GetRefreshInterval( configuration );
        updateUrl = GetUpdateUrl( configuration );
        // ReSharper disable once AsyncVoidLambda
        timer = new Timer( async _ => await Refresh(), null, TimeSpan.Zero, refreshIntervalHours );
    }

    readonly Timer timer;
    readonly IHttpService http;
    readonly string updateUrl;
    internal IEnumerable<OrderLocation> Locations => _locations;
    internal IReadOnlyDictionary<Guid, OrderLocation> LocationsById => _locationsById;

    List<OrderLocation> _locations = [];
    Dictionary<Guid, OrderLocation> _locationsById = [];

    async Task Refresh()
    {
        Dictionary<Guid, OrderLocation> cache = [];

        if ((await http.TryGetObjRequest<List<OrderLocation>>( updateUrl ))
            .Fails( out Reply<List<OrderLocation>> reply )) {
            Console.WriteLine( reply.Message() );
            return;
        }

        _locations = reply.Data;
        _locationsById = [];

        foreach ( OrderLocation l in _locations )
            _locationsById.Add( l.Id, l );
    }
    static TimeSpan GetRefreshInterval( IConfiguration config ) =>
        config.GetSection( "Ordering:OrderLocationCache" ).GetValue<TimeSpan>( "RefreshInterval" );
    static string GetUpdateUrl( IConfiguration config ) =>
        config.GetSection( "Ordering:OrderLocationCache" )["OrderingLocationApiUrl"] ??
        throw new Exception( $"Failed to retrieve {nameof( updateUrl )} from config in order location cache background service." );
}