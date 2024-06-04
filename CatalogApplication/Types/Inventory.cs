namespace CatalogApplication.Types;

public sealed class Inventory
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid ProductId { get; set; } = Guid.Empty;
    public Guid WarehouseId { get; set; } = Guid.Empty;
    public int QuantityAvailable { get; set; }
}