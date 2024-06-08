using CatalogApplication.Types.Brands.Models;
using CatalogApplication.Types.Filters.Models;

namespace CatalogApplication.Types.Brands.Dtos;

internal readonly record struct BrandsReply(
    List<Brand> Brands,
    List<BrandCategory> BrandCategories )
{
    internal static BrandsReply Empty() =>
        new( [], [] );
}