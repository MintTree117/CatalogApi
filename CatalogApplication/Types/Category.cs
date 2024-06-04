namespace CatalogApplication.Types;

public sealed class Category : IEntity
{
    public Category() { }

    public Category( Guid id, string name )
    {
        Id = id;
        Name = name;
        
    }
    
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}