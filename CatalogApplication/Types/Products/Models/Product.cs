namespace CatalogApplication.Types.Products.Models;

internal sealed record Product(
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
    int NumberSold );