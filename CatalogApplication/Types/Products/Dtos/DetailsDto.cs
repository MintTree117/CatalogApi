namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct DetailsDto(
    Guid ProductId,
    Guid BrandId,
    List<Guid> CategoryIds,
    string Name,
    string Description,
    string Image,
    bool IsInStock,
    bool IsFeatured,
    float Rating,
    decimal Price,
    decimal SalePrice,
    string Xml );