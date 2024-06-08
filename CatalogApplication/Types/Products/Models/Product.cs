namespace CatalogApplication.Types.Products.Models;

internal readonly record struct Product(
    Guid Id,
    Guid BrandId,
    float Rating,
    bool IsInStock,
    bool IsFeatured,
    string Name,
    string Image,
    decimal Price,
    decimal SalePrice );