namespace CatalogApplication.Types;

public sealed class VendorCategoryId : IEntity
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public Guid CategoryId { get; set; }
}