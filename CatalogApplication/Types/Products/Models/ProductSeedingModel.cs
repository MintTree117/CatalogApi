namespace CatalogApplication.Types.Products.Models;

internal readonly record struct ProductSeedingModel(
    List<Product> Products,
    List<ProductCategory> ProductCategories,
    List<ProductDescription> ProductDescriptions,
    List<ProductXml> ProductXmls );