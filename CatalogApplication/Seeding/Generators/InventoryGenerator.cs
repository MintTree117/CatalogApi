using System.Data;
using CatalogApplication.Types.Products.Models;
using CatalogApplication.Types.Stock;

namespace CatalogApplication.Seeding.Generators;

internal static class InventoryGenerator
{
    internal static List<ProductInventory> GenerateInventories( List<Product> products, List<Warehouse> warehouses, RandomUtility random )
    {
        List<ProductInventory> inventories = [];

        foreach ( Product p in products )
            foreach ( Warehouse w in warehouses )
            {
                int quantity = random.GetRandomInt( 1000 );
                inventories.Add( new ProductInventory( p.Id, w.Id, quantity ) );
            }
        
        return inventories;
    }
    
    internal static DataTable GenerateInventoryTable( List<ProductInventory> inventories )
    {
        DataTable table = new();
        table.Columns.Add( nameof( ProductInventory.ProductId ), typeof( Guid ) );
        table.Columns.Add( nameof( ProductInventory.WarehouseId ), typeof( Guid ) );
        table.Columns.Add( nameof( ProductInventory.Quantity ), typeof( int ) );

        foreach ( ProductInventory i in inventories )
        {
            DataRow row = table.NewRow();
            row[nameof( ProductInventory.ProductId )] = i.ProductId;
            row[nameof( ProductInventory.WarehouseId )] = i.WarehouseId;
            row[nameof( ProductInventory.Quantity )] = i.Quantity;
            table.Rows.Add( row );
        }

        return table;
    }
}