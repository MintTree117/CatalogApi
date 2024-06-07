using System.Data;
using CatalogApplication.Database;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Filters.Models;
using CatalogApplication.Types.ReplyTypes;
using Dapper;

namespace CatalogTests.Seeding.Utils;

internal static class BrandSeedUtils
{
    public static async Task<(Replies<Brand>, Replies<BrandCategory>)> SeedBrands( IDapperContext dapper, List<Category> categories, RandomUtility random )
    {
        const string tvpBrands = "BrandsTvp";
        const string tvpBrandCategories = "BrandCategoriesTvp";
        const string brandsSql =
            """
            INSERT INTO Brands (Id, Name)
            SELECT Id, Name
            FROM @BrandsTvp;
            """;
        const string brandCategorySql =
            """
            INSERT INTO BrandCategories (Id, BrandId, CategoryId)
            SELECT Id, BrandId, CategoryId)
            FROM @BrandCategoriesTvp;
            """;

        List<Brand> brands = GenerateBrands();
        List<BrandCategory> brandCategories = GenerateBrandCategories( brands, categories, random );

        DataTable brandsTable = GenerateBrandsTable( brands );
        DataTable brandCategoriesTable = GenerateBrandCategoriesTable( brandCategories );

        DynamicParameters brandsParameters = new();
        brandsParameters.Add( tvpBrands, brandsTable.AsTableValuedParameter( tvpBrands ) );
        DynamicParameters brandCategoriesParameters = new();
        brandCategoriesParameters.Add( tvpBrandCategories, brandCategoriesTable.AsTableValuedParameter( tvpBrandCategories ) );

        Reply<int> brandsReply = await dapper.ExecuteAsync( brandsSql, brandsParameters );
        if (!brandsReply.IsSuccess || brandsReply.Data <= 0)
            return (Replies<Brand>.With( brands ), Replies<BrandCategory>.None());
        
        Reply<int> brandCategoriesReply = await dapper.ExecuteAsync( brandCategorySql, brandCategoriesParameters );
        return brandCategoriesReply.IsSuccess && brandsReply.Data > 0
            ? (Replies<Brand>.With( brands ), Replies<BrandCategory>.With( brandCategories ))
            : (Replies<Brand>.With( brands ), Replies<BrandCategory>.None( brandCategoriesReply.Message() ));
    }
    static List<Brand> GenerateBrands()
    {
        List<Brand> brands = [];
        for ( int i = 0; i < 100; i++ )
            brands.Add( new Brand( Guid.NewGuid(), $"Brand {i}" ) );
        return brands;
    }
    static List<BrandCategory> GenerateBrandCategories( List<Brand> brands, List<Category> categories, RandomUtility random )
    {
        List<BrandCategory> brandCategories = [];

        foreach ( Brand b in brands ) {
            List<int> categoryIndices = random.GetRandomInts( categories.Count - 1, random.GetRandomInt( categories.Count * 2 ) );
            foreach ( int c in categoryIndices )
                brandCategories.Add( new BrandCategory( Guid.NewGuid(), b.Id, categories[c].Id ) );
        }

        return brandCategories;
    }
    static DataTable GenerateBrandsTable( List<Brand> brands )
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
    static DataTable GenerateBrandCategoriesTable( List<BrandCategory> brandCategories )
    {
        DataTable table = new();
        table.Columns.Add( nameof( BrandCategory.Id ), typeof( Guid ) );
        table.Columns.Add( nameof( BrandCategory.BrandId ), typeof( Guid ) );
        table.Columns.Add( nameof( BrandCategory.CategoryId ), typeof( Guid ) );

        foreach ( BrandCategory b in brandCategories ) {
            DataRow row = table.NewRow();
            row[nameof( BrandCategory.Id )] = b.Id;
            row[nameof( BrandCategory.BrandId )] = b.BrandId;
            row[nameof( BrandCategory.CategoryId )] = b.CategoryId;
            table.Rows.Add( row );
        }

        return table;
    }
}