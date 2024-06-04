namespace CatalogApplication.Types;

public sealed class BrandCategoryId : IEntity
{
    public Guid Id { get; set; }
    public Guid BrandId { get; set; }
    public Guid CategoryId { get; set; }
}