namespace CatalogApplication.Types;

public sealed class Subcategory : IEntity
{
    public Subcategory() { }

    public Subcategory( Guid id, Guid categoryId, string name )
    {
        Id = id;
        CategoryId = categoryId;
        Name = name;
    }

    public Guid Id { get; set; } = Guid.Empty;
    public Guid CategoryId { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
}