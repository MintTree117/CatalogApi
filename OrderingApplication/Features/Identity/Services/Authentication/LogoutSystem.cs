using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OrderingApplication.Features.Identity.Services.Account;
using OrderingApplication.Features.Identity.Types.Tokens;
using OrderingApplication.Features.Identity.Utilities;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Identity.Services.Authentication;

internal sealed class LogoutSystem( IdentityConfigCache config, RevokedTokenCache revoked )
{
    readonly JwtConfig _jwtConfig = config.JwtConfigRules;
    readonly RevokedTokenCache _revoked = revoked;
    
    internal async Task<Reply<bool>> Logout( ClaimsPrincipal accessTokenClaims, string refreshToken )
    {
        Reply<RevokedToken> accessReply = IdentityTokenUtils.GetRevokedToken( accessTokenClaims.Claims.ToList() );
        Reply<RevokedToken> refreshReply = Reply<RevokedToken>.None();

        if (IdentityTokenUtils.ParseToken( refreshToken, _jwtConfig, out JwtSecurityToken? token ).IsSuccess)
            refreshReply = IdentityTokenUtils.GetRevokedToken( token!.Claims.ToList() );
        if (accessReply.IsSuccess)
            await _revoked.AddRevokedToken( accessReply.Data );
        if (refreshReply.IsSuccess)
            await _revoked.AddRevokedToken( refreshReply.Data );

        return refreshReply.IsSuccess
            ? IReply.Okay()
            : IReply.None( "Failed to revoke a refresh token on the server." );
    }
}