namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct SearchDto(
    Guid ProductId,
    Guid BrandId,
    string Name,
    string Image,
    float Rating,
    decimal Price,
    decimal? SalePrice );