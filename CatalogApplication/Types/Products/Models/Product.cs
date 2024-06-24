namespace CatalogApplication.Types.Products.Models;

internal record Product(
    Guid Id,
    Guid PrimaryCategoryId,
    Guid BrandId,
    bool IsFeatured,
    bool IsInStock,
    string Name,
    string Image,
    decimal Price,
    decimal SalePrice,
    int NumberSold,
    int NumberRatings,
    float Rating );