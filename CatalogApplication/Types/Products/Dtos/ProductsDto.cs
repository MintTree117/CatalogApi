namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductsDto(
    List<ProductDetailsDto> Products,
    List<int> Estimates );