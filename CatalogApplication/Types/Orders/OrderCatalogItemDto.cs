namespace CatalogApplication.Types.Orders;

internal sealed class OrderCatalogItemDto
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public int Quantity { get; set; }
}