namespace CatalogApplication.Types.Brands.Models;

internal readonly record struct BrandCategory(
    Guid Id,
    Guid BrandId,
    Guid CategoryId );