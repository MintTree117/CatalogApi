namespace CatalogApplication.Types.Products.Dtos;

internal readonly record struct ProductSuggestionDto(
    Guid ProductId,
    string ProductName );