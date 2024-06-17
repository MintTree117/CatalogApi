namespace CatalogApplication.Types;

internal readonly record struct Pagination(
    int Page,
    int PageSize,
    string SortBy )
{
    internal int Offset() =>
        Math.Max( 0, Page ) * PageSize;
}