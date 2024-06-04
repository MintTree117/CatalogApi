namespace OrderingApplication.Features.Identity.Types.Login;

internal readonly record struct LoginRefreshReply(
    string AccessToken )
{
    internal static LoginRefreshReply Refreshed( string accessToken ) =>
        new( accessToken );
}