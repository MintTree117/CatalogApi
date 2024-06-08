namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct SearchFiltersDto(
    List<Guid>? BrandIds,
    int? MinimumPrice,
    int? MaximumPrice,
    int? MinimumRating,
    bool? IsInStock,
    bool? IsFeatured,
    bool IsOnSale );