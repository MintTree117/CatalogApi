namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductDto(
    Guid ProductId,
    Guid BrandId,
    List<Guid> CategoryIds,
    string Name,
    string Image,
    bool IsInStock,
    bool IsFeatured,
    decimal Price,
    decimal SalePrice,
    string Description,
    string Xml );