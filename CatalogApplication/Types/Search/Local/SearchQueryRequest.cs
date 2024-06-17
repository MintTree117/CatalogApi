using CatalogApplication.Types.Search.Dtos;

namespace CatalogApplication.Types.Search.Local;

internal readonly record struct SearchQueryRequest(
    string? SearchText,
    List<Guid>? CategoryIds,
    SearchFilters? ProductSearchFilters,
    Pagination Pagination );