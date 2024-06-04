using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using OrderingApplication.Features.Identity.Types.Tokens;
using OrderingDomain.Identity;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Identity.Utilities;

internal static class IdentityTokenUtils
{
    internal static string GenerateAccessToken( UserAccount user, JwtConfig jwtConfig )
    {
        DateTime expiration = DateTime.UtcNow + jwtConfig.AccessLifetime;
        SigningCredentials credentials = new( jwtConfig.Key, SecurityAlgorithms.HmacSha256 );
        Claim[] claims = [
            new Claim( JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() ), // Generate a new GUID
            new Claim( JwtRegisteredClaimNames.Exp, new DateTimeOffset( expiration ).ToUnixTimeSeconds().ToString() ), // Convert to Unix time
            new Claim( ClaimTypes.NameIdentifier, user.Id ),
            new Claim( ClaimTypes.Name, user.UserName ?? "No Username Found." )
        ];
        JwtSecurityToken token = new(
            null, // single issuer, no need
            jwtConfig.Audience,
            claims,
            expires: expiration,
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken( token );
    }
    internal static string GenerateAccessToken( TokenMeta meta, JwtConfig jwtConfig )
    {
        DateTime expiration = DateTime.UtcNow + jwtConfig.AccessLifetime;
        SigningCredentials credentials = new( jwtConfig.Key, SecurityAlgorithms.HmacSha256 );
        Claim[] claims = [
            new Claim( JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() ), // Generate a new GUID
            new Claim( JwtRegisteredClaimNames.Exp, new DateTimeOffset( expiration ).ToUnixTimeSeconds().ToString() ), // Convert to Unix time
            new Claim( ClaimTypes.NameIdentifier, meta.UserId ),
            new Claim( ClaimTypes.Name, meta.UserName )
        ];
        JwtSecurityToken token = new(
            null, // single issuer, no need
            jwtConfig.Audience,
            claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken( token );
    }
    internal static string GenerateRefreshToken( UserAccount user, JwtConfig jwtConfig )
    {
        DateTime expiration = DateTime.UtcNow + jwtConfig.RefreshLifetime;
        SigningCredentials credentials = new( jwtConfig.Key, SecurityAlgorithms.HmacSha256 );
        Claim[] claims = [
            new Claim( JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString() ), // Generate a new GUID
            new Claim( JwtRegisteredClaimNames.Exp, new DateTimeOffset( expiration ).ToUnixTimeSeconds().ToString() ), // Convert to Unix time
            new Claim( ClaimTypes.NameIdentifier, user.Id ),
            new Claim( ClaimTypes.Name, user.UserName ?? "No Username Found." )
        ];
        JwtSecurityToken token = new(
            null, // single issuer, no need
            jwtConfig.Audience,
            claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken( token );
    }
    internal static Reply<bool> ParseToken( string? token, JwtConfig jwtConfig, out JwtSecurityToken? securityToken )
    {
        try {
            securityToken = null;
            
            if (string.IsNullOrWhiteSpace( token ))
                return IReply.None();
            
            new JwtSecurityTokenHandler().ValidateToken( token, new TokenValidationParameters {
                ValidateIssuerSigningKey = jwtConfig.ValidateIssuerSigningKey,
                IssuerSigningKey = jwtConfig.Key,
                ValidateIssuer = jwtConfig.ValidateIssuer,
                ValidateAudience = jwtConfig.ValidateAudience,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                ClockSkew = TimeSpan.Zero // remove delay of token when expire
            }, out SecurityToken validatedToken );
            securityToken = (JwtSecurityToken) validatedToken;
            return IReply.Okay();
        }
        catch ( Exception e ) {
            securityToken = null;
            Console.WriteLine( e );
            return Reply<bool>.Exception( e, "An exception occurred while validating a json web token." );
        }
    }
    internal static Reply<RevokedToken> GetRevokedToken( List<Claim> claims )
    {
        Claim? j = claims.FirstOrDefault( c => c.Type == JwtRegisteredClaimNames.Jti );
        Claim? e = claims.FirstOrDefault( c => c.Type == JwtRegisteredClaimNames.Exp );
        
        if (!Guid.TryParse( j?.Value, out Guid id ))
            return Reply<RevokedToken>.None( "Failed to parse token id." );
        if (!long.TryParse( e?.Value, out long expiryUnixTime ))
            return Reply<RevokedToken>.None( "Failed to parse token expiry." );

        DateTime expiry = DateTimeOffset.FromUnixTimeSeconds( expiryUnixTime ).UtcDateTime;
        return Reply<RevokedToken>.With( RevokedToken.With( id, expiry ) );
    }
    internal static Reply<TokenMeta> ParseTokenMeta( string token, JwtConfig jwtConfig )
    {
        Reply<bool> result = ParseToken( token, jwtConfig, out JwtSecurityToken? jwt );
        if (!result.IsSuccess)
            return Reply<TokenMeta>.None();

        string? userId = jwt?.Claims.FirstOrDefault( x => x.Type == ClaimTypes.NameIdentifier )?.Value;
        string? username = jwt?.Claims.FirstOrDefault( x => x.Type == ClaimTypes.Name )?.Value;
        string? tokenId = jwt?.Claims.FirstOrDefault( x => x.Type == JwtRegisteredClaimNames.Jti )?.Value;

        if (!Guid.TryParse( tokenId, out Guid id ))
            return Reply<TokenMeta>.None( "Failed to parse token." );

        DateTime? expiryDate = jwt?.ValidTo;

        return userId is null || tokenId is null || expiryDate is null
            ? Reply<TokenMeta>.None( "Failed to parse token." )
            : Reply<TokenMeta>.With( TokenMeta.New( userId, username ?? string.Empty, id, expiryDate.Value ) );
    }
    internal static RevokedToken GetRevokedToken( TokenMeta meta )
    {
        return RevokedToken.With( meta.TokenId, meta.ExpiryDate );
    }
}