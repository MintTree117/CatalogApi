namespace OrderingApplication.Features.Identity.Types.Password;

internal readonly record struct ResetPasswordRequest(
    string Email,
    string Code,
    string NewPassword,
    string ConfirmPassword );