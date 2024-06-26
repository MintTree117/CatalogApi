using CatalogApplication.Types.Brands.Models;

namespace CatalogApplication.Types.Brands.Dtos;

internal readonly record struct BrandsDto(
    List<Brand> Brands,
    Dictionary<Guid, HashSet<Guid>> BrandCategories )
{
    internal static BrandsDto Empty() =>
        new( [], [] );
}