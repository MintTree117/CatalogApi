namespace CatalogApplication.Types;

public sealed class Vendor : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}