using Microsoft.AspNetCore.Identity;
using OrderingApplication.Features.Identity.Types.Accounts;
using OrderingApplication.Features.Identity.Utilities;
using OrderingDomain.Identity;
using OrderingDomain.Optionals;
using OrderingInfrastructure.Email;

namespace OrderingApplication.Features.Identity.Services.Account;

internal sealed class ProfileSystem( IdentityConfigCache configCache, UserManager<UserAccount> users, IEmailSender emailSender )
{
    readonly IdentityConfigCache _configCache = configCache;
    readonly UserManager<UserAccount> _users = users;
    readonly IEmailSender _emailSender = emailSender;
    
    // VIEW
    internal async Task<Reply<ViewAccountReply>> ViewIdentity( string userId ) =>
        (await _users.FindById( userId ))
        .Succeeds( out Reply<UserAccount> findById )
            ? ViewSuccess( findById.Data )
            : ViewFailure( findById );
    static Reply<ViewAccountReply> ViewSuccess( UserAccount user ) =>
        Reply<ViewAccountReply>.With( ViewAccountReply.With( user ) );
    static Reply<ViewAccountReply> ViewFailure( IReply result ) =>
        Reply<ViewAccountReply>.None( result );
    
    // UPDATE
    internal async Task<Reply<bool>> UpdateAccount( string userId, UpdateAccountRequest update )
    {
        if ((await _users.FindById( userId )).Fails( out var user ))
            return IReply.None( "User not found." );

        if (ValidateUpdate( user.Data, update ).Fails( out Reply<bool> validationResult ))
            return IReply.None( validationResult );
        
        IdentityResult updateResult = await _users.UpdateAsync( user.Data );
        return updateResult.Succeeds()
                ? IReply.Okay()
                : IReply.None( $"Failed to save changes to account. {updateResult.CombineErrors()}" );
    }
    Reply<bool> ValidateUpdate( UserAccount user, UpdateAccountRequest update ) =>
        UpdateEmail( user, update.Email ).Succeeds( out var managedResult ) &&
        UpdatePhone( user, update.Phone ).Succeeds( out managedResult ) &&
        UpdateUsername( user, update.Username ).Succeeds( out managedResult ) &&
        UpdateTwoFactor( user, update.HasTwoFactor ).Succeeds( out managedResult )
            ? IReply.Okay()
            : IReply.None( managedResult );
    Reply<bool> UpdateEmail( UserAccount user, string newEmail )
    {
        if (!IdentityValidationUtils.ValidateEmail( newEmail, _configCache.EmailRules ).Succeeds( out var result ))
            return result;

        user.Email = newEmail;
        return IReply.Okay();
    }
    Reply<bool> UpdatePhone( UserAccount user, string? newPhone )
    {
        if (!IdentityValidationUtils.ValidatePhone( newPhone, _configCache.PhoneRules ).Succeeds( out var result ))
            return result;

        user.PhoneNumber = newPhone;
        return IReply.Okay();
    }
    Reply<bool> UpdateUsername( UserAccount user, string newUsername )
    {
        if (!IdentityValidationUtils.ValidateUsername( newUsername, _configCache.UsernameRules ).Succeeds( out var result ))
            return result;

        user.UserName = newUsername;
        return IReply.Okay();
    }
    Reply<bool> UpdateTwoFactor( UserAccount user, bool enable )
    {
        user.TwoFactorEnabled = enable;
        return IReply.Okay();
    }
    
    // DELETE
    internal async Task<Reply<bool>> DeleteIdentity( string userId, DeleteAccountRequest request )
    {
        if ((await _users.FindById( userId )).Fails( out var user ))
            return IReply.None( user );

        if (!await _users.CheckPasswordAsync( user.Data, request.Password ))
            return IReply.None( "Invalid password." );

        return (await _users.DeleteAsync( user.Data )).Succeeds()
            ? SendDeletionEmail( user, _emailSender )
            : DeleteFailure();
    }
    static Reply<bool> SendDeletionEmail( Reply<UserAccount> user, IEmailSender sender )
    {
        const string subject = "Account Deleted";
        string body = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>{subject}</title>
            </head>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                    <h2 style='color: #333;'>{subject}</h2>
                    <p>Dear {user.Data.UserName},</p>
                    <p>We regret to see you go. This is to confirm that you have successfully deleted your account. If this was a mistake, please contact our support team as soon as possible.</p>
                    <p>If you have any feedback or questions, feel free to reach out to us.</p>
                    <p>Best regards,<br/>The Team</p>
                </div>
            </body>
            </html>";
        return sender.SendHtmlEmail( user.Data.Email ?? string.Empty, "Account deleted", body );
    }
    static Reply<bool> DeleteFailure() =>
        IReply.None( "Failed to delete account due to internal server error." );
}