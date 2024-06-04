namespace CatalogApplication.Types.Filters.Models;

internal readonly record struct BrandCategory(
    Guid Id,
    Guid BrandId,
    Guid CategoryId );