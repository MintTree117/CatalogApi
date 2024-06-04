namespace OrderingApplication.Features.Identity.Types.Accounts;

internal readonly record struct UpdateAccountRequest(
    string Username,
    string Email,
    string? Phone,
    bool HasTwoFactor = false );