namespace CatalogApplication.Types.Filters.Models;

internal readonly record struct PriceRange(
    Guid Id,
    int MinimumDollars,
    int MaximumDollars );