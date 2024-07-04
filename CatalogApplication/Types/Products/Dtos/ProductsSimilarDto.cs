namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductsSimilarDto(
    List<ProductSummaryDto> SimilarToBrand,
    List<ProductSummaryDto> SimilarToProduct );