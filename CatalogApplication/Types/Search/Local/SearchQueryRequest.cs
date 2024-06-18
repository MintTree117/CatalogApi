namespace CatalogApplication.Types.Search.Local;

internal readonly record struct SearchQueryRequest(
    string? SearchText,
    Guid? CategoryId,
    SearchFilters? ProductSearchFilters,
    Pagination Pagination );