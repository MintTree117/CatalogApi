namespace CatalogApplication.Types.Products.Models;

internal readonly record struct ProductCategory(
    Guid Id,
    Guid ProductId,
    Guid CategoryId );