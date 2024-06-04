namespace CatalogApplication.Types;

public sealed class Brand : IEntity
{
    public Brand() { }

    public Brand( Guid id, string name )
    {
        Id = id;
        Name = name;
    }
    
    public Guid Id { get; set; } = Guid.Empty;
    public List<BrandCategoryId> BrandCategoryIds { get; set; } = [ ];
    public string Name { get; set; } = string.Empty;
}