using System.Data;
using CatalogApplication.Database;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.ReplyTypes;
using Dapper;

namespace CatalogApplication.Seed;

internal static class CategorySeeder
{
    // TODO: Move to legitimate database
    public static Replies<Category> SeedCategoriesInMemory( RandomUtility random )
    {
        List<Category> categories = GenerateCategoriesStatic();
        return Replies<Category>.With( categories );
    }
    public static async Task<Replies<Category>> SeedCategories( IDapperContext dapper, RandomUtility random )
    {
        const string tvpName = "CategoriesTvp";
        const string sql =
            """
            INSERT INTO Categories (Id, ParentId, Name)
            SELECT ID, ParentId, Name
            FROM @CategoriesTvp
            """;
        
        List<Category> categories = GenerateCategoriesStatic();
        DataTable tableParam = GenerateCategoriesTable( categories );
        DynamicParameters parameters = new();
        parameters.Add( tvpName, tableParam.AsTableValuedParameter( tvpName ) );

        Reply<int> result = await dapper.ExecuteAsync( sql, parameters );
        return result.IsSuccess && result.Data > 0
            ? Replies<Category>.With( categories )
            : Replies<Category>.None( result );
    }
    static List<Category> GenerateCategoriesRandom( RandomUtility random )
    {
        const int PrimaryCount = 9;
        const int MaxSubcount = 12;

        List<Category> primaryCategories = [];
        for ( int i = 0; i < PrimaryCount; i++ )
            primaryCategories.Add( new Category( Guid.NewGuid(), null, $"Category {i}" ) );

        List<Category> subCategories = [];
        foreach ( Category c in primaryCategories ) {
            int subcount = random.GetRandomInt( MaxSubcount );
            for ( int i = 0; i < subcount; i++ )
                subCategories.Add( new Category( Guid.NewGuid(), c.Id, $"{c.Name} Subcategory {i}" ) );
        }
        
        List<Category> mergedCategories = [..primaryCategories];
        mergedCategories.AddRange( subCategories );
        return mergedCategories;
    }
    static List<Category> GenerateCategoriesStatic()
    {
        List<Category> categories = [];

        int pcIndex = 0;
        foreach ( string s in SeedData.PrimaryCategories.Keys ) {
            Category pc = new( Guid.NewGuid(), null, s );
            foreach ( string s2 in SeedData.SubCategories[pcIndex].Keys ) {
                Category sc = new( Guid.NewGuid(), pc.Id, s2 );
                categories.Add( sc );
            }
            categories.Add( pc );
            pcIndex++;
        }
        
        return categories;
    }
    static DataTable GenerateCategoriesTable( List<Category> categories )
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