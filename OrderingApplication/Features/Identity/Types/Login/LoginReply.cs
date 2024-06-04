namespace OrderingApplication.Features.Identity.Types.Login;

internal readonly record struct LoginReply(
    string? AccessToken,
    string? RefreshToken,
    bool? IsPending2FA )
{
    internal static LoginReply LoggedIn( string access, string refresh ) => 
        new( access, refresh, false );
    internal static LoginReply Pending2FA() => 
        new( null, null, true );
}