namespace CatalogApplication.Types.Search.Local;

internal readonly record struct SearchFilters(
    string? SearchText,
    Guid? CategoryId,
    List<Guid>? BrandIds,
    int? MinPrice,
    int? MaxPrice,
    bool? IsInStock,
    bool? IsFeatured,
    bool IsOnSale,
    int Page,
    int PageSize,
    int SortBy );