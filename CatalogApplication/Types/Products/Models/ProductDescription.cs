namespace CatalogApplication.Types.Products.Models;

internal readonly record struct ProductDescription(
    Guid Id,
    Guid ProductId,
    string Description );