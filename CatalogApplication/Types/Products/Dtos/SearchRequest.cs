using CatalogApplication.Types.Filters.Dtos;

namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct SearchRequest(
    List<Guid>? CategoryIds,
    SearchFiltersDto? ProductSearchFilters,
    Pagination Pagination );