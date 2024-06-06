namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct Pagination(
    int Page,
    int Rows,
    string OrderBy )
{
    internal int Offset() =>
        Math.Max( 0, Page ) * Rows;
}