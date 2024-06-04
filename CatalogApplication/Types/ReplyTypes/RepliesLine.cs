namespace CatalogApplication.Types.ReplyTypes;

public readonly record struct RepliesLine<T>(
    List<Reply<T>> Options ) where T : class, new()
{
    public bool Succeeds( out RepliesLine<T> self )
    {
        self = this;
        return !AnyFailed( out self );
    }
    public bool AnySucceeded => Options.Any( o => o.IsSuccess );
    public bool AnyFailed( out RepliesLine<T> self )
    {
        self = this;
        return Options.Any( o => !o.IsSuccess );
    }
    public List<T> ToObjects() => Options.Select( o => o.Data ).ToList();
}