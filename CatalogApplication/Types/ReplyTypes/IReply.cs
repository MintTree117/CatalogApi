namespace CatalogApplication.Types.ReplyTypes;

public interface IReply
{
    public string Message();
    public bool IsSuccess { get; init; }

    public static Reply<bool> Okay() => Reply<bool>.With( true );
    public static Reply<bool> None() => Reply<bool>.None();
    public static Reply<bool> None( string message ) => Reply<bool>.None( message );
    public static Reply<bool> None( IReply other ) => Reply<bool>.None( other );
}