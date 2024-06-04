namespace OrderingApplication.Features.Identity.Types.Login;

internal readonly record struct TwoFactorRequest(
    string EmailOrUsername,
    string Code );