namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductDetailsDto(
    Guid ProductId,
    Guid BrandId,
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