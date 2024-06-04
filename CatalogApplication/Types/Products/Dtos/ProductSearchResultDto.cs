namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductSearchResultDto(
    Guid ProductId,
    Guid BrandId,
    Guid ShippingTimespanId,
    string Name,
    string Image,
    float Rating,
    decimal Price,
    decimal? SalePrice );