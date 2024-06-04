using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Identity.Services.Account;
using OrderingApplication.Features.Identity.Types.Login;
using OrderingApplication.Features.Identity.Types.Tokens;
using OrderingApplication.Features.Identity.Utilities;
using OrderingDomain.Identity;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Identity.Services.Authentication;

internal sealed class LoginRefreshSystem( IdentityConfigCache configCache, RevokedTokenCache revoked, UserManager<UserAccount> userManager )
{
    readonly JwtConfig _jwtConfig = configCache.JwtConfigRules;
    readonly RevokedTokenCache _revoked = revoked;
    readonly UserManager<UserAccount> _userManager = userManager;

    internal async Task<Reply<bool>> CheckLogin( string accessToken )
    {
        Reply<TokenMeta> metaReply = IdentityTokenUtils.ParseTokenMeta( accessToken, _jwtConfig );
        if (!metaReply.IsSuccess)
            return IReply.None( "Invalid Token." );

        if (_revoked.IsTokenRevoked( metaReply.Data.TokenId ))
            return IReply.None( "Invalid Token." );

        if (DateTime.UtcNow > metaReply.Data.ExpiryDate)
            return IReply.None( "Session Has Expired." );
        
        Reply<UserAccount> userReply = await _userManager.FindById( metaReply.Data.UserId );
        return userReply.IsSuccess
            ? IReply.Okay()
            : IReply.None( "User not found." );
    } 
    internal async Task<Reply<LoginRefreshReply>> LoginRefresh( LoginRefreshRequest request )
    {
        // Revoke old access token if exists
        if (!string.IsNullOrWhiteSpace( request.AccessToken ))
            await TryRevokeToken( request.AccessToken, _jwtConfig, _revoked );
        
        // Try parsing the refresh token
        Reply<TokenMeta> metaReply = IdentityTokenUtils.ParseTokenMeta( request.RefreshToken, _jwtConfig );
        if (!metaReply.IsSuccess)
            return TokenInvalid( metaReply );
        
        // Check expiry
        if (DateTime.UtcNow > metaReply.Data.ExpiryDate)
            return Reply<LoginRefreshReply>.None( "Session Has Expired." );
        
        // Return a new access token if the refresh isn't revoked
        return _revoked.IsTokenRevoked( metaReply.Data.TokenId )
            ? TokenInvalid( metaReply )
            : TokenRefreshed( metaReply, _jwtConfig );
        
        // Utils
        static Reply<LoginRefreshReply> TokenInvalid( IReply reply ) =>
            Reply<LoginRefreshReply>.None( $"Invalid authentication token. {reply.Message()}" );
        static Reply<LoginRefreshReply> TokenRefreshed( Reply<TokenMeta> meta, JwtConfig config ) =>
            Reply<LoginRefreshReply>.With( LoginRefreshReply.Refreshed( IdentityTokenUtils.GenerateAccessToken( meta.Data, config ) ) );
    }
    internal async Task<Reply<LoginReply>> LoginRefreshFull( LoginRefreshRequest request )
    {
        // Revoke old access token if exists
        if (!string.IsNullOrWhiteSpace( request.AccessToken ))
            await TryRevokeToken( request.AccessToken, _jwtConfig, _revoked );

        // Try parsing the refresh token
        Reply<TokenMeta> metaReply = IdentityTokenUtils.ParseTokenMeta( request.RefreshToken, _jwtConfig );
        if (!metaReply.IsSuccess)
            return Reply<LoginReply>.None( $"Invalid authentication token. {metaReply.Message()}" );
        
        // Revoke the refresh token
        await TryRevokeToken( request.RefreshToken, _jwtConfig, _revoked );
        
        // Create new login using user, because old refresh has old data
        Reply<UserAccount> userReply = await _userManager.FindById( metaReply.Data.UserId );
        if (!userReply.IsSuccess)
            return Reply<LoginReply>.None( "User not found." );
        
        // Return a new login
        string accessToken = IdentityTokenUtils.GenerateAccessToken( userReply.Data, _jwtConfig );
        string refreshToken = IdentityTokenUtils.GenerateRefreshToken( userReply.Data, _jwtConfig );
        return Reply<LoginReply>.With( LoginReply.LoggedIn( accessToken, refreshToken ) );
    }

    static async Task TryRevokeToken( string token, JwtConfig config, RevokedTokenCache revoked )
    {
        Reply<TokenMeta> metaReply = IdentityTokenUtils.ParseTokenMeta( token, config );
        
        if (!metaReply.IsSuccess)
            return;

        RevokedToken revokedToken = IdentityTokenUtils.GetRevokedToken( metaReply.Data );
        await revoked.AddRevokedToken( revokedToken );
    }
}