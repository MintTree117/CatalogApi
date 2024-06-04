using OrderingApplication.Features.Identity.Services.Account;
using OrderingApplication.Features.Identity.Types.Tokens;
using OrderingDomain.Optionals;
using OrderingInfrastructure.Http;

namespace OrderingApplication.Features.Identity.Services.Authentication;

internal sealed class RevokedTokenBroadcaster
{
    readonly IHttpService _http;
    readonly Dictionary<RevokedToken, HashSet<string>> _failed = [];
    readonly object _lockObject = new();
    readonly Timer _timer;
    readonly string[] _otherServerUrls;
    
    public RevokedTokenBroadcaster( IHttpService http, IdentityConfigCache configCache )
    {
        _http = http;
        _otherServerUrls = configCache.PeerServers;
        _timer = new Timer( TimerCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes( 1 ) );
    }
    internal async Task<Reply<bool>> BroadcastRevoke( RevokedToken token )
    {
        foreach ( string url in _otherServerUrls ) {
            if (!await TryBroadcast( token, url ))
                HandleBroadcastFail( token, url );
        }

        return _failed.ContainsKey( token )
            ? IReply.None( "Not all servers were able to be notified of logout." )
            : IReply.Okay();
    }
    
    async void TimerCallback( object? _ )
    {
        await Cleanup();
    }
    async Task Cleanup()
    {
        List<RevokedToken> tokens = _failed.Keys.ToList();

        foreach ( RevokedToken token in tokens ) {
            if (DateTime.Now > token.ExpiryDate)
                _failed.Remove( token );
        }

        foreach ( RevokedToken token in tokens ) {
            if (!_failed.ContainsKey( token ))
                continue;
            foreach ( string url in _failed[token] )
                if (await TryBroadcast( token, url ))
                    _failed[token].Remove( url );
        }

        foreach ( RevokedToken token in tokens ) {
            if (!_failed.ContainsKey( token ))
                continue;
            if (_failed[token].Count <= 0)
                _failed.Remove( token );
        }
    }
    async Task<bool> TryBroadcast( RevokedToken token, string url )
    {
        Reply<bool> result = await _http.TryPostObjRequest<bool>( url, token );
        return result.IsSuccess;
    }
    void HandleBroadcastFail( RevokedToken token, string url )
    {
        if (!_failed.TryGetValue( token, out HashSet<string>? urls )) {
            urls = [];
            _failed.Add( token, urls );
        }

        urls.Add( url );
    }
}