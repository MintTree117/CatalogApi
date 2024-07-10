namespace CatalogApplication.Types.Orders;

internal sealed class OrderCatalogItemDto
{
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal UnitDiscount { get; set; }
    public decimal ShippingCost { get; set; }
    public int Quantity { get; set; }
}