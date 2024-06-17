namespace CatalogApplication.Types.Products.Models;

internal record ProductSeedingModel(
    List<Product> Products,
    List<ProductCategory> ProductCategories,
    List<ProductDescription> ProductDescriptions,
    List<ProductXml> ProductXmls );