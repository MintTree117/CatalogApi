namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct SearchFiltersDto(
    List<Guid>? BrandIds,
    List<Guid>? PriceRangeIds,
    List<Guid>? RatingLevelIds,
    bool? IsInStock,
    bool? IsFeatured,
    bool? IsOnSale );