namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct DetailsDto(
    Guid ProductId,
    Guid BrandId,
    Guid PriceRangeId,
    Guid RatingLevelId,
    Guid ShippingTimespanId,
    List<Guid> CategoryIds,
    bool IsFeatured,
    string Name,
    string Description,
    string Image,
    float Rating,
    decimal Price,
    decimal? SalePrice,
    string Xml );