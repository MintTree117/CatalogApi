namespace CatalogApplication.Types.Products.Models;

internal readonly record struct ProductKeywords(
    Guid ProductId,
    string Keywords );