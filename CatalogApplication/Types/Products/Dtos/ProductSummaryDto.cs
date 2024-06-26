namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductSummaryDto(
    Guid Id,
    Guid BrandId,
    string Name,
    string Image,
    bool IsFeatured,
    bool IsInStock,
    decimal Price,
    decimal SalePrice,
    int NumberRatings,
    float Rating );