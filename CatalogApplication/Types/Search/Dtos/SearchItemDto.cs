namespace CatalogApplication.Types.Search.Dtos;

internal readonly record struct SearchItemDto(
    Guid Id,
    Guid BrandId,
    string Name,
    string Image,
    bool IsFeatured,
    bool IsInStock,
    decimal Price,
    decimal? SalePrice,
    int NumberSold,
    int NumberRatings,
    float Rating );