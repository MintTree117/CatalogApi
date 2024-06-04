namespace CatalogApplication.Types;

public sealed class Warehouse
{
    public Guid WarehouseId { get; set; } = Guid.Empty;
    public Guid AddressId { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
}