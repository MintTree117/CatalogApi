namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductSpecialsDto(
    List<ProductDetailsDto> TopFeatured,
    List<ProductDetailsDto> TopSales,
    List<ProductDetailsDto> TopSelling );