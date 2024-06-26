namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct SpecialsDto(
    List<ProductDto> TopFeatured,
    List<ProductDto> TopSales,
    List<ProductDto> TopSelling );