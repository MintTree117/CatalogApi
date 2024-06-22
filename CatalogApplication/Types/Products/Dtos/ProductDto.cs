namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductDto(
    Guid Id,
    Guid BrandId,
    string Name,
    string Image,
    bool IsFeatured,
    bool IsInStock,
    int ShippingDays,
    decimal Price,
    decimal SalePrice,
    int NumberRatings,
    float Rating,
    List<Guid>? CategoryIds,
    string? Description,
    string? Xml );