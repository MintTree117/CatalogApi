namespace OrderingApplication.Features.Identity.Types.Tokens;

internal readonly record struct TokenMeta(
    string UserId,
    string UserName,
    Guid TokenId,
    DateTime ExpiryDate )
{
    internal static TokenMeta New( string u, string n, Guid t, DateTime e ) =>
        new( u, n, t, e );
}