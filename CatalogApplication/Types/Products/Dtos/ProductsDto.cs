namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductsDto(
    List<ProductSummaryDto> Products,
    List<int> Estimates );