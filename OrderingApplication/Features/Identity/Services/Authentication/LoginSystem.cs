using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Identity.Services.Account;
using OrderingApplication.Features.Identity.Types.Login;
using OrderingApplication.Features.Identity.Types.Tokens;
using OrderingApplication.Features.Identity.Utilities;
using OrderingDomain.Identity;
using OrderingDomain.Optionals;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Identity.Services.Authentication;

internal sealed class LoginSystem( IdentityConfigCache configCache, UserManager<UserAccount> userManager, IEmailSender emailSender )
{
    readonly JwtConfig _jwtConfig = configCache.JwtConfigRules;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;

    readonly bool _requiresConfirmedEmail = userManager.Options.SignIn.RequireConfirmedEmail;
    readonly int _maxAccessFailCount = userManager.Options.Lockout.MaxFailedAccessAttempts;
    readonly TimeSpan _lockoutTimespan = userManager.Options.Lockout.DefaultLockoutTimeSpan;

    // LOGIN
    internal async Task<Reply<LoginReply>> Login( LoginRequest request )
    {
        Reply<UserAccount> userReply = await ValidateLogin( request );
        return !userReply.IsSuccess
            ? await HandleLoginFail( userReply )
            : await Is2FaRequired( userReply )
                ? await HandleRequires2Fa( userReply )
                : HandleNormalLogin( userReply );
        
        async Task<bool> Is2FaRequired( Reply<UserAccount> user ) =>
            _userManager.SupportsUserTwoFactor && await _userManager.GetTwoFactorEnabledAsync( user.Data );
    }
    async Task<Reply<UserAccount>> ValidateLogin( LoginRequest login )
    {
        Reply<bool> validationResult = IReply.Okay();
        
        bool validated =
            (await _userManager.FindByEmailOrUsername( login.EmailOrUsername )).Succeeds( out Reply<UserAccount> userResult ) &&
            (await IsAccountValid( userResult )).Succeeds( out validationResult ) &&
            (await IsPasswordValid( userResult, login )).Succeeds( out validationResult ) &&
            (await ClearAccessFailCount( userResult )).Succeeds( out validationResult );

        return validated
            ? userResult
            : Reply<UserAccount>.None( $"{userResult.Message()} : {validationResult.Message()}" );
    }
    async Task<Reply<bool>> IsPasswordValid( Reply<UserAccount> u, LoginRequest login )
    {
        bool success = await _userManager.CheckPasswordAsync( u.Data, login.Password );
        return success
            ? IReply.Okay()
            : IReply.None( "Invalid password." );
    }
    async Task<Reply<bool>> ClearAccessFailCount( Reply<UserAccount> user )
    {
        IdentityResult result = await _userManager.ResetAccessFailedCountAsync( user.Data );
        return result.Succeeded
            ? IReply.Okay()
            : IReply.None( $"Failed to reset access count: {result.CombineErrors()}" );
    }
    async Task<Reply<LoginReply>> HandleLoginFail( Reply<UserAccount> user )
    {
        IReply processReply = await ProcessAccessFailure( user.Data, user.Message() );
        return Reply<LoginReply>.None( processReply );
    }
    async Task<Reply<LoginReply>> HandleRequires2Fa( Reply<UserAccount> user )
    {
        bool generated2FA =
            (await Set2FaToken( user.Data, _userManager )).Succeeds( out var problem ) &&
            (await Send2FaEmail( user.Data, _emailSender )).Succeeds( out problem );

        return generated2FA
            ? Reply<LoginReply>.With( LoginReply.Pending2FA() )
            : Reply<LoginReply>.None( problem );
    }
    async Task<Reply<bool>> Send2FaEmail( UserAccount user, IEmailSender email )
    {
        const string Header = "Verify your login";
        string code = IdentityValidationUtils.Encode( await _userManager.GenerateTwoFactorTokenAsync( user, "Email" ) );
        string body = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>{Header}</title>
            </head>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>{Header}</h2>
                    <p>Dear {user.UserName},</p>
                    <p>Your two-factor verification code is:</p>
                    <p style='font-size: 24px; font-weight: bold;'>{code}</p>
                    <p>Please enter this code to verify your login. If you did not request this, please ignore this email.</p>
                    <p>Best regards,<br/>The Team</p>
                </div>
            </body>
            </html>";
        
        return email.SendHtmlEmail( user.Email ?? string.Empty, Header, body );
    }
    Reply<LoginReply> HandleNormalLogin( Reply<UserAccount> user )
    {
        Reply<Tokens> generateReply = GenerateLoginTokens( user );
        return generateReply.IsSuccess
            ? Reply<LoginReply>.With( LoginReply.LoggedIn( generateReply.Data.AccessToken, generateReply.Data.RefreshToken ) )
            : Reply<LoginReply>.None( "Failed to generate login credentials." );
    }
    
    // TWO FACTOR
    internal async Task<Reply<TwoFactorReply>> Login2Factor( TwoFactorRequest request )
    {
        Reply<UserAccount> user = await Validate2Factor( request );
        Reply<Tokens> tokenResult = Reply<Tokens>.None();

        return user.IsSuccess && GenerateLoginTokens( user ).Succeeds( out tokenResult )
            ? Reply<TwoFactorReply>.With(
                TwoFactorReply.Authenticated( tokenResult.Data.AccessToken, tokenResult.Data.RefreshToken ) )
            : Reply<TwoFactorReply>.None(
                await ProcessAccessFailure( user.Data, $"{user.Message()} : {tokenResult.Message()}" ) );
    }
    async Task<Reply<UserAccount>> Validate2Factor( TwoFactorRequest twoFactor )
    {
        Reply<bool> validationResult = IReply.Okay();
        bool validated =
            (await _userManager.FindByEmailOrUsername( twoFactor.EmailOrUsername )).Succeeds( out Reply<UserAccount> userResult ) &&
            (await IsAccountValid( userResult )).Succeeds( out validationResult ) &&
            (await IsTwoFactorValid( userResult, twoFactor )).Succeeds( out validationResult );
        
        return validated
            ? userResult
            : Reply<UserAccount>.None( $"{userResult.Message()} : {validationResult.Message()}" );
    }
    async Task<Reply<bool>> IsTwoFactorValid( Reply<UserAccount> user, TwoFactorRequest twoFactor )
    {
        bool valid =
            !string.IsNullOrWhiteSpace( twoFactor.Code ) &&
            await _userManager.VerifyTwoFactorTokenAsync( user.Data, "Email", twoFactor.Code );

        return valid
            ? IReply.Okay()
            : IReply.None( "Access token is invalid." );
    }
    static async Task<Reply<bool>> Set2FaToken( UserAccount user, UserManager<UserAccount> users )
    {
        IdentityResult result = await users.SetAuthenticationTokenAsync( user, "Email", "Two Factor Token", await users.GenerateTwoFactorTokenAsync( user, "Email" ) );

        return result.Succeeded
            ? IReply.Okay()
            : IReply.None( "Failed to set authentication token." );
    }
    
    // SHARED UTILS
    async Task<Reply<bool>> IsAccountValid( Reply<UserAccount> user )
    {
        bool emailConfirmed = !_requiresConfirmedEmail || await _userManager.IsEmailConfirmedAsync( user.Data );
        
        if (!emailConfirmed)
            return Reply<bool>.None( "Your account is not confirmed. Please check your email for a confirmation link." );

        if (await _userManager.IsLockedOutAsync( user.Data ))
            return Reply<bool>.None( "Your account is locked. Please try again later." );

        return IReply.Okay();
    }
    async Task<IReply> ProcessAccessFailure( UserAccount account, string message )
    {
        if (account is null)
            return IReply.None( "User not found." );
        
        await _userManager.AccessFailedAsync( account );

        if (await _userManager.GetAccessFailedCountAsync( account ) > _maxAccessFailCount)
            await _userManager.SetLockoutEndDateAsync( account, DateTime.Now + _lockoutTimespan );

        return IReply.None( message );
    }
    Reply<Tokens> GenerateLoginTokens( Reply<UserAccount> user )
    {
        string accessToken = IdentityTokenUtils.GenerateAccessToken( user.Data, _jwtConfig );
        string refreshToken = IdentityTokenUtils.GenerateRefreshToken( user.Data, _jwtConfig );
        return Reply<Tokens>.With( new Tokens( accessToken, refreshToken ) );
    }
    
    readonly record struct Tokens(
        string AccessToken,
        string RefreshToken );
}