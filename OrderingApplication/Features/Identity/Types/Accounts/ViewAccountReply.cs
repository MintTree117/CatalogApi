using OrderingDomain.Identity;

namespace OrderingApplication.Features.Identity.Types.Accounts;

internal readonly record struct ViewAccountReply(
    string? Username,
    string? Email,
    string? Phone,
    bool HasTwoFactor )
{
    internal static ViewAccountReply With( UserAccount user ) => new(
        user.UserName,
        user.Email,
        user.PhoneNumber,
        user.TwoFactorEnabled );
}