namespace CatalogApplication.Types.Products.Models;

internal readonly record struct ProductXml(
    Guid Id,
    Guid ProductId,
    string Xml );