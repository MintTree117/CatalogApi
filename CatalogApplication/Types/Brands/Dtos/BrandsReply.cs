using CatalogApplication.Types.Brands.Models;

namespace CatalogApplication.Types.Brands.Dtos;

internal readonly record struct BrandsReply(
    List<Brand> Brands,
    List<BrandCategory> BrandCategories )
{
    internal static BrandsReply Empty() =>
        new( [], [] );
}