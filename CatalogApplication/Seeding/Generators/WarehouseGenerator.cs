using System.Data;
using CatalogApplication.Seeding.SeedData;
using CatalogApplication.Types.Stock;

namespace CatalogApplication.Seeding.Generators;

internal static class WarehouseGenerator
{
    internal static List<Warehouse> GenerateWarehouses()
    {
        List<Warehouse> warehouses = [];

        for ( int i = 0; i < WarehouseSeedData.WarehouseCells.Length; i++ )
            warehouses.Add( new Warehouse( 
                Guid.NewGuid(), WarehouseSeedData.WarehouseUrls[i], WarehouseSeedData.WarehouseCells[i].X, WarehouseSeedData.WarehouseCells[i].Y ) );
        
        return warehouses;
    }

    internal static DataTable GenerateWarehousesTable( List<Warehouse> warehouses )
    {
        DataTable table = new();
        table.Columns.Add( nameof( Warehouse.Id ), typeof( Guid ) );
        table.Columns.Add( nameof( Warehouse.QueryUrl ), typeof( string ) );
        table.Columns.Add( nameof( Warehouse.PosX ), typeof( int ) );
        table.Columns.Add( nameof( Warehouse.PosY ), typeof( int ) );

        foreach ( Warehouse w in warehouses ) {
            DataRow row = table.NewRow();
            row[nameof( Warehouse.Id )] = w.Id;
            row[nameof( Warehouse.QueryUrl )] = w.QueryUrl;
            row[nameof( Warehouse.PosX )] = w.PosX;
            row[nameof( Warehouse.PosY )] = w.PosY;
            table.Rows.Add( row );
        }

        return table;
    }
}