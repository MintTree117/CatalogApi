using CatalogApplication.Types.Products.Dtos;

namespace CatalogApplication.Types.Search.Dtos;

internal readonly record struct SearchProductsDto(
    List<ProductDto> Products,
    List<int> Estimates );