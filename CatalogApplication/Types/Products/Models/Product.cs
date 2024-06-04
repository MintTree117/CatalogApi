namespace CatalogApplication.Types.Products.Models;

internal readonly record struct Product(
    Guid Id,
    Guid BrandId,
    Guid PriceRangeId,
    Guid RatingLevelId,
    Guid ShippingTimespanId,
    bool IsFeatured,
    bool IsOnSale,
    string Name,
    string Image,
    float Rating,
    decimal Price,
    decimal? SalePrice );