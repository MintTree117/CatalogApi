namespace CatalogApplication.Types.Filters.Models;

internal readonly record struct ShippingTimespan(
    Guid Id,
    TimeSpan MinimumDays,
    TimeSpan MaximumDays );