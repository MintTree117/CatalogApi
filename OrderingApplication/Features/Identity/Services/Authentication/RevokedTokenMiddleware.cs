using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace OrderingApplication.Features.Identity.Services.Authentication;

internal sealed class RevokedTokenMiddleware( RequestDelegate next, ILogger<RevokedTokenMiddleware> logger, RevokedTokenCache revokedTokenCache )
{
    readonly RequestDelegate _next = next;
    readonly ILogger<RevokedTokenMiddleware> _logger = logger;
    readonly RevokedTokenCache _revokedTokenCache = revokedTokenCache;

    public async Task InvokeAsync( HttpContext context )
    {
        Endpoint? endpoint = context.GetEndpoint();
        bool process = endpoint?.Metadata.GetMetadata<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>() != null;
        
        if (process && TokenIsRevoked( context ))
            return;
        
        await _next( context );
    }

    bool TokenIsRevoked( HttpContext http )
    {
        try {
            ClaimsPrincipal user = http.User;

            if (user.Identity is null || !user.Identity.IsAuthenticated) {
                http.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return true;
            }

            Claim? tokenIdClaim = user.Claims.FirstOrDefault( c => c.Type == JwtRegisteredClaimNames.Jti );

            bool isRevoked =
                tokenIdClaim == null ||
                !Guid.TryParse( tokenIdClaim.Value, out Guid tokenId ) ||
                _revokedTokenCache.IsTokenRevoked( tokenId );

            if (!isRevoked)
                return false;
            
            http.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return true;
        }
        catch ( Exception ex ) {
            _logger.LogError( ex, "An error occurred while processing the token revocation middleware." );
            http.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return false;
        }
        
    }
}
