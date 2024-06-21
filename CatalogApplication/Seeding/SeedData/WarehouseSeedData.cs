using CatalogApplication.Types._Common.Geography;

namespace CatalogApplication.Seeding.SeedData;

internal static class WarehouseSeedData
{
    internal static string[] WarehouseUrls = [
        "",
        "",
        "",
        "",
        "",
        "",
        "",
        "",
        "",
        "",
    ];
    internal static readonly Cell[] WarehouseCells = new Cell[] {
        new ( 0, 0 ),
        new ( 100, 100 ),
        new ( 1000, 1000 ),
        new ( 10000, 10000 ),
        new ( 100000, 100000 ),
        new ( 1000000, 1000000 ),
        new ( 10000000, 10000000 )
    };
}