namespace CatalogApplication.Types.Search.Local;

internal readonly record struct SearchFilters(
    List<Guid>? BrandIds,
    int? MinPrice,
    int? MaxPrice,
    bool? IsInStock,
    bool? IsFeatured,
    bool IsOnSale );