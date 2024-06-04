namespace OrderingApplication.Features.Identity.Types.Login;

internal readonly record struct LoginRefreshRequest(
    string? AccessToken,
    string RefreshToken );