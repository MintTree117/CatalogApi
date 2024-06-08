namespace CatalogApplication.Types.Products.Models;

internal readonly record struct ProductCategory(
    Guid ProductId,
    Guid CategoryId );