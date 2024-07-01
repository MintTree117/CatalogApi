namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductsSearchDto(
    int TotalMatches,
    List<ProductSummaryDto> Results )
{
    internal static ProductsSearchDto With( int count, List<ProductSummaryDto> dtos ) =>
        new( count, dtos );
}