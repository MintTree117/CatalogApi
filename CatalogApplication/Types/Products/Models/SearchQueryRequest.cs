using CatalogApplication.Types.Products.Dtos;

namespace CatalogApplication.Types.Products.Models;

internal readonly record struct SearchQueryRequest(
    List<Guid>? CategoryIds,
    SearchFiltersDto? ProductSearchFilters,
    Pagination Pagination );