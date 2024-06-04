namespace OrderingInfrastructure;

public readonly record struct PagedResult<T>(
    int TotalCount,
    List<T> Items )
{
    public static PagedResult<T> With( int count, List<T> items ) =>
        new( count, items );
}