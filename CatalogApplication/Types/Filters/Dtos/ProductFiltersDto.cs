using CatalogApplication.Types.Filters.Models;

namespace CatalogApplication.Types.Filters.Dtos;

internal readonly record struct ProductFiltersDto(
    List<Brand> Brands, 
    List<BrandCategory> BrandCategories,
    List<PriceRange> PriceRanges,
    List<RatingLevel> RatingLevels,
    List<ShippingTimespan> ShippingTimespans );