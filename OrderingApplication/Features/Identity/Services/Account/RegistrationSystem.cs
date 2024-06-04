using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Identity.Types.Registration;
using OrderingApplication.Features.Identity.Utilities;
using OrderingDomain.Identity;
using OrderingDomain.Optionals;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Identity.Services.Account;

internal sealed class RegistrationSystem( IdentityConfigCache configCache, UserManager<UserAccount> userManager, IEmailSender emailSender )
{
    readonly IdentityConfigCache _configCache = configCache;
    readonly UserManager<UserAccount> _userManager = userManager;
    readonly IEmailSender _emailSender = emailSender;
    
    internal async Task<Reply<RegisterReply>> RegisterIdentity( RegisterRequest register )
    {
        if (IdentityValidationUtils.ValidateRegistration( register, _configCache ).Fails( out var validation ))
            return RegistrationFail( "Failed to register account." + validation.Message() );

        if ((await CreateIdentity( register )).Fails( out var user ))
            return RegistrationFail( "Failed to register account." );

        return ((await SendConfirmationEmail( user ))
            .IsSuccess
            ? RegistrationSuccess( user )
            : RegistrationFail( "Internal server error. Failed to register account." ));
    }
    internal async Task<Reply<bool>> ConfirmEmail( ConfirmEmailRequest request )
    {
        if ((await FindIdentity( request )).Fails( out Reply<UserAccount> user ))
            return IReply.None( user.Message() );

        if ((await _userManager.IsEmailConfirmedAsync( user.Data )))
            return IReply.None( "Email is already confirmed." );
        
        return (await ConfirmEmail( user, request )).IsSuccess
            ? IReply.Okay()
            : IReply.None( "Email confirmation code is invalid." );
    }
    internal async Task<Reply<bool>> ResendConfirmLink( ConfirmResendRequest request )
    {
        if ((await FindIdentity( request )).Fails( out Reply<UserAccount> user ))
            return IReply.None( user.Message() );

        if ((await _userManager.IsEmailConfirmedAsync( user.Data )))
            return IReply.None( "Email is already confirmed." );

        return (await SendConfirmationEmail( user )).IsSuccess
            ? IReply.Okay()
            : IReply.None( "Failed to resend confirmation link." );
    }

    async Task<Reply<UserAccount>> FindIdentity( ConfirmEmailRequest request ) =>
        await _userManager.FindByEmail( request.Email );
    async Task<Reply<UserAccount>> FindIdentity( ConfirmResendRequest request ) =>
        await _userManager.FindByEmail( request.Email );
    async Task<Reply<UserAccount>> CreateIdentity( RegisterRequest request )
    {
        UserAccount user = new( request.Email, request.Username );
        return (await _userManager.CreateAsync( user, request.Password ))
            .Succeeds( out IdentityResult result )
                ? Reply<UserAccount>.With( user )
                : Reply<UserAccount>.None( $"Failed to create user. {result.CombineErrors()}" );
    }
    async Task<Reply<bool>> SendConfirmationEmail( Reply<UserAccount> user )
    {
        string code = IdentityValidationUtils.Encode( await _userManager.GenerateEmailConfirmationTokenAsync( user.Data ) );
        string returnPage = _configCache.ConfirmEmailPage;
        string body = GenerateConfirmEmailBody( user.Data, code, _configCache.ConfirmEmailPage );
        return _emailSender
               .SendHtmlEmail( user.Data.Email ?? string.Empty, "Confirm your email", body )
               .Succeeds( out var emailResult )
            ? IReply.Okay()
            : IReply.None( emailResult );
    }
    async Task<Reply<bool>> ConfirmEmail( Reply<UserAccount> user, ConfirmEmailRequest request ) =>
        (await _userManager.ConfirmEmailAsync( user.Data, IdentityValidationUtils.Decode( request.Code ) )).Succeeds( out IdentityResult result )
            ? IReply.Okay()
            : IReply.None();

    static string GenerateConfirmEmailBody( UserAccount user, string token, string returnUrl )
    {
        string link = $"{returnUrl}?email={user.Email}&code={token}";
        string html = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Email Confirmation</title>
            </head>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>Email Confirmation</h2>
                    <p>Dear {user.UserName},</p>
                    <p>Thank you for registering with us. Please click the link below to confirm your email address:</p>
                    <p>
                        <a href='{link}' style='display: inline-block; padding: 10px 20px; font-size: 16px; color: #fff; background-color: #007BFF; text-decoration: none; border-radius: 5px;'>Confirm Email</a>
                    </p>
                    <p>If you did not create an account, please ignore this email.</p>
                    <p>Best regards,<br/>The Team</p>
                </div>
            </body>
            </html>";
        return html;
    }
    static Reply<RegisterReply> RegistrationFail( string message ) =>
        Reply<RegisterReply>.None( message );
    static Reply<RegisterReply> RegistrationSuccess( Reply<UserAccount> user ) =>
        Reply<RegisterReply>.With( RegisterReply.With( user.Data ) );
}