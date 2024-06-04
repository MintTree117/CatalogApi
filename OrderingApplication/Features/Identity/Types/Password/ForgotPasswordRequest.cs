namespace OrderingApplication.Features.Identity.Types.Password;

internal readonly record struct ForgotPasswordRequest(
    string Email );