namespace OrderingApplication.Features.Identity.Types.Login;

internal readonly record struct LoginRequest(
    string EmailOrUsername,
    string Password );