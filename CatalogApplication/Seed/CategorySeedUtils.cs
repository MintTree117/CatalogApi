using System.Data;
using CatalogApplication.Database;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.ReplyTypes;
using CatalogTests;
using Dapper;

namespace CatalogApplication.Seed;

internal static class CategorySeedUtils
{
    // TODO: Move to legitimate database
    public static Replies<Category> SeedCategoriesInMemory( RandomUtility random )
    {
        List<Category> categories = GenerateCategories( random );
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
        
        List<Category> categories = GenerateCategories( random );
        DataTable tableParam = GenerateCategoriesTable( categories );
        DynamicParameters parameters = new();
        parameters.Add( tvpName, tableParam.AsTableValuedParameter( tvpName ) );

        Reply<int> result = await dapper.ExecuteAsync( sql, parameters );
        return result.IsSuccess && result.Data > 0
            ? Replies<Category>.With( categories )
            : Replies<Category>.None( result );
    }
    static List<Category> GenerateCategories( RandomUtility random )
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