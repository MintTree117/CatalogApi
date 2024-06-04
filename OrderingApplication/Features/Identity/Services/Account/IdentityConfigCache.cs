using System.Text;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using OrderingApplication.Features.Identity.Types.Password;
using OrderingApplication.Features.Identity.Types.Tokens;
using OrderingApplication.Extentions;

namespace OrderingApplication.Features.Identity.Services.Account;

internal sealed class IdentityConfigCache( IConfiguration configuration )
{
    internal readonly string[] PeerServers = GetPeerServers( configuration );
    
    internal readonly string ConfirmEmailPage = configuration.GetOrThrow( $"Identity:Pages:{nameof( ConfirmEmailPage )}" );
    internal readonly string ResetPasswordPage = configuration.GetOrThrow( $"Identity:Pages:{nameof( ResetPasswordPage )}" );

    internal readonly Regex EmailRules = GetEmailRules( configuration );
    internal readonly Regex UsernameRules = GetUsernameRules( configuration );
    internal readonly Regex PhoneRules = GetPhoneRules( configuration );
    internal readonly PasswordConfig PasswordConfigRules = GetPasswordRules( configuration );
    internal readonly JwtConfig JwtConfigRules = GetJwtRules( configuration );

    static string[] GetPeerServers( IConfiguration configuration ) =>
        configuration.GetSection( $"Identity:{nameof( PeerServers )}" ).Get<string[]>() ?? throw new Exception( $"Failed to get {nameof( PeerServers )} from identity configuration." );
    static Regex GetEmailRules( IConfiguration configuration ) => 
        new( configuration.GetOrThrow( "Identity:Validation:EmailRegex" ) );
    static Regex GetUsernameRules( IConfiguration configuration ) => 
        new( configuration.GetOrThrow( "Identity:Validation:UsernameRegex" ) );
    static Regex GetPhoneRules( IConfiguration configuration ) => 
        new( configuration.GetOrThrow( "Identity:Validation:PhoneRegex" ) );
    static PasswordConfig GetPasswordRules( IConfiguration configuration ) =>
        configuration.GetSection( "Identity:Validation:PasswordCriteria" ).Get<PasswordConfig>() ?? 
        throw configuration.Exception( nameof( PasswordConfig ) );
    static JwtConfig GetJwtRules( IConfiguration config ) => new() {
        Key = new SymmetricSecurityKey( Encoding.UTF8.GetBytes( config.GetOrThrow( "Identity:Jwt:Key" ) ) ),
        Audience = config.GetOrThrow( "Identity:Jwt:Audience" ),
        Issuer = config.GetOrThrow( "Identity:Jwt:Issuer" ),
        ValidateAudience = config.GetSection( "Identity:Jwt:ValidateAudience" ).Get<bool>(),
        ValidateIssuer = config.GetSection( "Identity:Jwt:ValidateIssuer" ).Get<bool>(),
        ValidateIssuerSigningKey = config.GetSection( "Identity:Jwt:ValidateIssuerSigningKey" ).Get<bool>(),
        AccessLifetime = TimeSpan.Parse( config.GetOrThrow( "Identity:Jwt:AccessLifetime" ) ),
        RefreshLifetime = TimeSpan.Parse( config.GetOrThrow( "Identity:Jwt:RefreshLifetime" ) )
    };
}