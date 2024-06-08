namespace CatalogApplication.Types.Products.Models;

internal readonly record struct ProductTag(
    Guid ProductId,
    Guid TagId );