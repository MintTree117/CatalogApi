using CatalogApplication.Types.Products.Dtos;

namespace CatalogApplication.Types.Products.Models;

internal readonly record struct SearchQueryRequest(
    string? SearchText,
    List<Guid>? CategoryIds,
    SearchFiltersDto? ProductSearchFilters,
    Pagination Pagination );