namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductDetailsDto(
    Guid Id,
    Guid BrandId,
    bool IsFeatured,
    bool IsInStock,
    string Name,
    string Image,
    decimal Price,
    decimal SalePrice,
    float Rating,
    int NumberRatings,
    int ShippingDays,
    List<Guid>? CategoryIds,
    string? Description,
    string? Xml );