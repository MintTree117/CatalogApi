using System.Data;
using CatalogApplication.Seeding.SeedData;
using CatalogApplication.Types.Brands.Models;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Filters.Models;

namespace CatalogApplication.Seeding.Generators;

internal static class BrandGenerator
{
    const int MaxCategoriesPerBrand = 6;
    
    internal static List<Brand> GenerateBrands()
    {
        HashSet<string> brandNames = [];
        foreach ( string[] brandsList in BrandSeedData.BrandsPerCategory.Values )
            foreach ( string b in brandsList )
                brandNames.Add( b );

        List<Brand> brands = [];
        foreach ( string b in brandNames )
            brands.Add( new Brand( Guid.NewGuid(), b ) );
        
        return brands;
    }
    internal static List<BrandCategory> GenerateBrandCategories( List<Brand> brands, List<Category> categories )
    {
        List<BrandCategory> brandCategories = [];

        foreach ( Brand b in brands ) {
            foreach ( KeyValuePair<string, string[]> kvp in BrandSeedData.BrandsPerCategory ) {
                if (!kvp.Value.Contains( b.Name ))
                    continue;
                Category category = categories.First( c => c.Name == kvp.Key );
                brandCategories.Add( new BrandCategory( b.Id, category.Id ) );
                break;
            }
        }

        return brandCategories;
    }
    internal static DataTable GenerateBrandsTable( List<Brand> brands )
    {
        DataTable table = new();
        table.Columns.Add( nameof( Brand.Id ), typeof( Guid ) );
        table.Columns.Add( nameof( Brand.Name ), typeof( string ) );

        foreach ( Brand b in brands ) {
            DataRow row = table.NewRow();
            row[nameof( Brand.Id )] = b.Id;
            row[nameof( Brand.Name )] = b.Name;
            table.Rows.Add( row );
        }

        return table;
    }
    internal static DataTable GenerateBrandCategoriesTable( List<BrandCategory> brandCategories )
    {
        DataTable table = new();
        table.Columns.Add( nameof( BrandCategory.BrandId ), typeof( Guid ) );
        table.Columns.Add( nameof( BrandCategory.CategoryId ), typeof( Guid ) );

        foreach ( BrandCategory b in brandCategories ) {
            DataRow row = table.NewRow();
            row[nameof( BrandCategory.BrandId )] = b.BrandId;
            row[nameof( BrandCategory.CategoryId )] = b.CategoryId;
            table.Rows.Add( row );
        }

        return table;
    }
}