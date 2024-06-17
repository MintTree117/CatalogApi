namespace CatalogApplication.Types._Common.ReplyTypes;

public readonly record struct RepliesLine<T>(
    List<Reply<T>> Options ) where T : class, new()
{
    public bool Succeeds( out RepliesLine<T> self )
    {
        self = this;
        return !AnyFailed( out self );
    }
    public bool AnySucceeded => Options.Any( static o => o );
    public bool AnyFailed( out RepliesLine<T> self )
    {
        self = this;
        return Options.Any( static o => !o );
    }
    public List<T> ToObjects() => Options.Select( static o => o.Data ).ToList();
}