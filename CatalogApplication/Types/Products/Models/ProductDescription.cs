namespace CatalogApplication.Types.Products.Models;

internal readonly record struct ProductDescription(
    Guid ProductId,
    string Description );