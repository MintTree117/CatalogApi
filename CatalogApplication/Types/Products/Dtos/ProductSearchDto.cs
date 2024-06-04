namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductSearchDto(
    int TotalMatches,
    List<ProductSearchResultDto> Results );