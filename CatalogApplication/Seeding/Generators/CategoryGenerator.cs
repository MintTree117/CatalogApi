using System.Data;
using CatalogApplication.Seeding.SeedData;
using CatalogApplication.Types.Categories;

namespace CatalogApplication.Seeding.Generators;

internal static class CategoryGenerator
{
    internal static List<Category> GenerateCategories()
    {
        List<Category> categories = [];

        int pcIndex = 0;
        foreach ( string s in CategorySeedData.PrimaryCategories ) {
            Category pc = new( Guid.NewGuid(), null, s );
            foreach ( string s2 in CategorySeedData.SecondaryCategories[pcIndex].Keys ) {
                Category sc = new( Guid.NewGuid(), pc.Id, s2 );
                categories.Add( sc );
            }
            categories.Add( pc );
            pcIndex++;
        }
        
        return categories;
    }
    internal static DataTable GenerateCategoriesTable( List<Category> categories )
    {
        DataTable table = new();
        table.Columns.Add( nameof( Category.Id ), typeof( Guid ) );
        table.Columns.Add( nameof( Category.ParentId ), typeof( Guid ) );
        table.Columns.Add( nameof( Category.Name ), typeof( string ) );

        foreach ( Category c in categories ) {
            DataRow row = table.NewRow();
            row[nameof( Category.Id )] = c.Id;
            row[nameof( Category.ParentId )] = c.ParentId;
            row[nameof( Category.Name )] = c.Name;
            table.Rows.Add( row );
        }

        return table;
    }
}