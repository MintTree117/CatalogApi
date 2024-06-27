namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductSpecialsDto(
    List<ProductSummaryDto> TopFeatured,
    List<ProductSummaryDto> TopSales,
    List<ProductSummaryDto> TopSelling );