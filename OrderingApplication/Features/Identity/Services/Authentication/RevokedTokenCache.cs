using OrderingApplication.Features.Identity.Types.Tokens;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Identity.Services.Authentication;

internal sealed class RevokedTokenCache
{
    readonly RevokedTokenBroadcaster _tokenBroadcaster;
    readonly object _lockObject = new();
    readonly Timer _timer;
    readonly Dictionary<Guid, DateTime> _revokedIds = []; // stored as primitives for efficient lookups, as this is used in middleware

    readonly ILogger<RevokedTokenCache> _logger;
    
    public RevokedTokenCache( RevokedTokenBroadcaster tokenBroadcaster, ILogger<RevokedTokenCache> logger )
    {
        _tokenBroadcaster = tokenBroadcaster;
        _logger = logger;
        _timer = new Timer( _ => Cleanup(), null, TimeSpan.Zero, TimeSpan.FromMinutes( 10 ) );
    }
    internal async Task<Reply<bool>> AddRevokedToken( RevokedToken token )
    {
        if (_revokedIds.ContainsKey( token.TokenId ))
            return IReply.Okay();
        _revokedIds.Add( token.TokenId, token.ExpiryDate );
        await _tokenBroadcaster.BroadcastRevoke( token );
        return IReply.Okay();
    }
    internal bool IsTokenRevoked( Guid tokenId )
    {
        return _revokedIds.ContainsKey( tokenId );
    }

    void Cleanup()
    {
        _logger.LogInformation( "RevokedTokenCache: Cleanup();" );

        try {
            lock ( _lockObject ) {
                var expiredTokens = _revokedIds.Where( pair => DateTime.Now > pair.Value ).ToList();
                foreach ( var pair in expiredTokens ) {
                    _revokedIds.Remove( pair.Key );
                }
            }
        }
        catch ( Exception e ) {
            _logger.LogError( e, $"An exception occured while executing RevokedTokenCache.Cleanup() : {e.Message}" );
            throw;
        }

    }
}