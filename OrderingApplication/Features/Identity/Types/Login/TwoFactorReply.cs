namespace OrderingApplication.Features.Identity.Types.Login;

internal readonly record struct TwoFactorReply(
    string AccessToken,
    string RefreshToken )
{
    internal static TwoFactorReply Authenticated( string access, string refresh ) =>
        new( access, refresh );
}