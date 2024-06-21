namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductDto(
    Guid Id,
    Guid BrandId,
    List<Guid> CategoryIds,
    string Name,
    string Image,
    bool IsFeatured,
    decimal Price,
    decimal SalePrice,
    int NumberSold,
    int NumberRatings,
    float Rating,
    string Description,
    string Xml );