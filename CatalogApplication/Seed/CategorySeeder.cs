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
        foreach ( string s in PrimaryCategories ) {
            Category pc = new( Guid.NewGuid(), null, s );
            foreach ( string s2 in SubCategories[pcIndex] ) {
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
    
    // Primary categories
    static string[] PrimaryCategories = [
        "Computers & Laptops",
        "Smartphones & Tablets",
        "TVs & Home Entertainment",
        "Cameras & Photography",
        "Audio & Headphones",
        "Wearable Technology",
        "Gaming",
        "Accessories",
        "Home Appliances",
        "Office Electronics"
    ];

    // Sub-categories for each primary category
    static string[][] SubCategories = [
        // Computers & Laptops
        [
            "Laptops",
            "Desktops",
            "Tablets",
            "Monitors",
            "Computer Accessories"
        ],

        // Smartphones & Tablets
        [
            "Smartphones",
            "Tablets",
            "Smartwatch",
            "Mobile Accessories"
        ],

        // TVs & Home Entertainment
        [
            "Televisions",
            "Home Theater Systems",
            "Media Players",
            "Projectors",
            "TV Accessories"
        ],

        // Cameras & Photography
        [
            "Digital Cameras",
            "DSLR Cameras",
            "Action Cameras",
            "Lenses & Accessories",
            "Camera Drones"
        ],

        // Audio & Headphones
        [
            "Headphones",
            "Speakers",
            "Home Audio Systems",
            "MP3 Players",
            "Audio Accessories"
        ],

        // Wearable Technology
        [
            "Smartwatches",
            "Fitness Trackers",
            "VR Headsets",
            "Wearable Accessories"
        ],

        // Gaming
        [
            "Gaming Consoles",
            "Video Games",
            "Gaming Accessories",
            "Gaming Laptops & PCs"
        ],

        // Accessories
        [
            "Cables & Adapters",
            "Cases & Covers",
            "Chargers & Power Banks",
            "Batteries",
            "Storage Devices"
        ],

        // Home Appliances
        [
            "Refrigerators",
            "Washing Machines",
            "Microwaves",
            "Vacuum Cleaners",
            "Kitchen Appliances"
        ],

        // Office Electronics
        [
            "Printers & Scanners",
            "Projectors",
            "Office Accessories",
            "Calculators",
            "Shredders"
        ]
    ];
}