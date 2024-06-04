namespace OrderingApplication.Features.Identity.Types.Tokens;

internal readonly record struct RevokedToken(
    Guid TokenId,
    DateTime ExpiryDate )
{
    internal static RevokedToken With( Guid id, DateTime expiry ) =>
        new( id, expiry );
}