namespace CatalogApplication.Types.Categories;

internal readonly record struct Category(
    Guid Id,
    Guid? ParentId,
    string Name );