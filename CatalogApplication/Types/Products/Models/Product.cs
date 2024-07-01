namespace CatalogApplication.Types.Products.Models;

internal sealed record Product(
    Guid Id,
    Guid BrandId,
    string Name,
    string BrandName, // de-normalized
    string Image,
    bool IsFeatured,
    bool IsInStock,
    decimal Price,
    decimal SalePrice,
    float Rating,
    int NumberRatings,
    int NumberSold );