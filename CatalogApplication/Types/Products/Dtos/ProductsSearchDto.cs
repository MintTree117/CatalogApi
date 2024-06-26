namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductsSearchDto(
    int TotalMatches,
    List<ProductSummaryDto> Results,
    List<int> ShippingDays );