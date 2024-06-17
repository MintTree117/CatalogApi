namespace CatalogApplication.Types.Categories;

internal record Category(
    Guid Id,
    Guid? ParentId,
    string Name );