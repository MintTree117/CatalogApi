using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Identity.Types.Password;
using OrderingApplication.Features.Identity.Utilities;
using OrderingDomain.Identity;
using OrderingDomain.Optionals;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Identity.Services.Account;

internal sealed class PasswordSystem( IdentityConfigCache identityConfigCache, UserManager<UserAccount> um, IEmailSender es )
{
    readonly IdentityConfigCache _configCache = identityConfigCache;
    readonly UserManager<UserAccount> userManager = um;
    readonly IEmailSender emailSender = es;
    
    internal async Task<Reply<bool>> ManagePassword( string userId, UpdatePasswordRequest request )
    {
        Reply<UserAccount> result = await FindUser( userId );
        return result.IsSuccess
            ? await ManagePassword( result.Data, request )
            : IReply.None( "User not found." );
    }
    internal async Task<Reply<bool>> ForgotPassword( ForgotPasswordRequest request )
    {
        Reply<UserAccount> result = await FindUser( request );
        return result.IsSuccess
            ? await SendResetEmail( result )
            : IReply.None( "User not found." );
    }
    internal async Task<Reply<bool>> ResetPassword( ResetPasswordRequest request )
    {
        if ((await FindUser( request )).Fails( out var user ))
            return IReply.None( user );

        return (await TryResetPassword( user, request )).Succeeded
            ? IReply.Okay()
            : IReply.None( "Internal server error. Failed to reset password." );
    }

    async Task<Reply<UserAccount>> FindUser( string userId ) =>
        await userManager.FindById( userId );
    async Task<Reply<UserAccount>> FindUser( ForgotPasswordRequest request ) =>
        await userManager.FindByEmail( request.Email );
    async Task<Reply<UserAccount>> FindUser( ResetPasswordRequest request ) =>
        await userManager.FindByEmail( request.Email );
    
    async Task<Reply<bool>> SendResetEmail( Reply<UserAccount> user ) =>
        emailSender
            .SendHtmlEmail( user.Data.Email!, "Reset your password", await GenerateResetEmailBody( user.Data ) )
            .Succeeds( out Reply<bool> result )
            ? IReply.Okay()
            : IReply.None( result );
    async Task<string> GenerateResetEmailBody( UserAccount user ) =>
        $"Please reset your password by <a href='{await GenerateResetLink( user )}'>clicking here</a>.";
    async Task<string> GenerateResetLink( UserAccount user ) =>
        $"{_configCache.ResetPasswordPage}?Email={user.Email}&Code={IdentityValidationUtils.Encode( await userManager.GeneratePasswordResetTokenAsync( user ) )}";
    async Task<IdentityResult> TryResetPassword( Reply<UserAccount> user, ResetPasswordRequest request ) =>
        await userManager.ResetPasswordAsync( user.Data, IdentityValidationUtils.Decode( request.Code ), request.NewPassword );
    async Task<Reply<bool>> ManagePassword( UserAccount user, UpdatePasswordRequest request )
    {
        if (string.IsNullOrWhiteSpace( request.NewPassword ))
            return IReply.None( "No replacement password provided." );

        if (IdentityValidationUtils.ValidatePassword( request.NewPassword, _configCache.PasswordConfigRules ).Fails( out var validated ))
            return validated;

        return (await userManager.ChangePasswordAsync( user, request.OldPassword, request.NewPassword ))
            .Succeeds( out IdentityResult result )
                ? IReply.Okay()
                : IReply.None( result.CombineErrors() );
    }
}