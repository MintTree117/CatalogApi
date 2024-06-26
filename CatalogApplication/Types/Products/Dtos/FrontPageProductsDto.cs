namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct FrontPageProductsDto(
    List<ProductDto> TopFeatured,
    List<ProductDto> TopSales,
    List<ProductDto> TopSelling );