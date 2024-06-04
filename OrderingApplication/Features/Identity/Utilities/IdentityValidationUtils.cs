using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;
using OrderingApplication.Features.Identity.Services.Account;
using OrderingApplication.Features.Identity.Types.Password;
using OrderingApplication.Features.Identity.Types.Registration;
using OrderingApplication.Features.Identity.Services;
using OrderingApplication.Features.Identity.Types;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Identity.Utilities;

internal static class IdentityValidationUtils
{
    internal static string Encode( string content ) =>
        WebEncoders.Base64UrlEncode( Encoding.UTF8.GetBytes( content ) );
    internal static string Decode( string content ) =>
        Encoding.UTF8.GetString( WebEncoders.Base64UrlDecode( content ) );
    
    internal static Reply<bool> ValidateRegistration( RegisterRequest r, IdentityConfigCache configCache ) => IReply.Okay();
    /*internal static Opt<bool> ValidateRegistration( RegisterRequest r, IdentityConfigurationConsts consts ) =>
        ValidateEmail( r.Email, consts.EmailRules ).Fail( out Opt<bool> opt ) ||
        ValidateUsername( r.Username, consts.UsernameRules ).Fail( out opt ) ||
        ValidatePhone( r.Phone, consts.PhoneRules ).Fail( out opt ) ||
        ValidatePassword( r.Password, consts.PasswordRules ).Fail( out opt )
            ? IOpt.None( $"Validation failed. {opt.Message()}" )
            : IOpt.Okay();*/
    internal static Reply<bool> ValidateEmail( string? email, Regex regex ) =>
        string.IsNullOrWhiteSpace( email ) || !regex.IsMatch( email )
            ? IReply.None( "Invalid email format." )
            : IReply.Okay();
    internal static Reply<bool> ValidateUsername( string? username, Regex regex ) =>
        string.IsNullOrWhiteSpace( username ) || !regex.IsMatch( username )
            ? IReply.None( "Invalid username format." )
            : IReply.Okay();
    internal static Reply<bool> ValidatePhone( string? phone, Regex regex ) =>
        !string.IsNullOrWhiteSpace( phone ) && !regex.IsMatch( phone )
            ? IReply.None( "Invalid phone number format." )
            : IReply.Okay();
    internal static Reply<bool> ValidatePassword( string? password, PasswordConfig criteria ) =>
        HandleNull( password ) &&
        HandleMinLength( password!, criteria.MinLength ) &&
        HandleMaxLength( password!, criteria.MaxLength ) &&
        HandleLowercase( password!, criteria.RequireLowercase ) &&
        HandleUppercase( password!, criteria.RequireUppercase ) &&
        HandleDigit( password!, criteria.RequireDigit ) &&
        HandleSpecial( password!, criteria.RequireSpecial, criteria.Specials )
            ? IReply.Okay()
            : IReply.None( "Invalid password." );

    static bool HandleNull( string? password ) =>
        !string.IsNullOrEmpty( password ) && !string.IsNullOrWhiteSpace( password );
    static bool HandleMinLength( string password, int min ) =>
        password.Length >= min;
    static bool HandleMaxLength( string password, int max ) =>
        password.Length <= max;
    static bool HandleLowercase( string password, bool requires ) =>
        !requires || password.Any( char.IsLower );
    static bool HandleUppercase( string password, bool requires ) =>
        !requires || password.Any( char.IsUpper );
    static bool HandleDigit( string password, bool requires ) =>
        !requires || password.Any( char.IsDigit );
    static bool HandleSpecial( string password, bool requires, string special ) => 
        !requires || Regex.IsMatch( password, $"[{Regex.Escape( special )}]" );
}