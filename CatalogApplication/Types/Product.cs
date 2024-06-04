namespace CatalogApplication.Types;

public sealed class Product : IEntity
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid CategoryId { get; set; } = Guid.Empty;
    public Guid BrandId { get; set; } = Guid.Empty;
    public bool IsFeatured { get; set; }
    public bool IsOnSale { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public decimal Price { get; set; }
}