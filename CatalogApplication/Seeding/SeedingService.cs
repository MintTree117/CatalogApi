using System.Data;
using CatalogApplication.Database;
using CatalogApplication.Seeding.Generators;
using CatalogApplication.Types.Brands.Models;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Filters.Models;
using CatalogApplication.Types.Products.Models;
using CatalogApplication.Types.ReplyTypes;
using Dapper;

namespace CatalogApplication.Seeding;

internal sealed class SeedingService
{
    const int DatabaseInsertPageSize = 100;
    const int DatabaseInsertPageSizeBigData = 25;
    
    readonly IDapperContext _dapper;
    readonly ILogger<SeedingService> _logger;
    static readonly RandomUtility _random = new();

    public SeedingService( IDapperContext dapper, ILogger<SeedingService> logger )
    {
        _dapper = dapper;
        _logger = logger;
    }
    internal async Task SeedDatabase()
    {
        // CLEAR CATALOG
        Reply<int> clearReply = await _dapper.ExecuteStoredProcedure( "ClearCatalog" );
        if (!clearReply.IsSuccess)
            throw new Exception( $"Failed to clear database during seeding: {clearReply.Message()}" );
        _logger.LogInformation( "Cleared Catalog." );
        
        // CATEGORIES
        Replies<Category> categoriesReply = await SeedCategories();
        if (!categoriesReply.IsSuccess)
            throw new Exception( $"Failed to generate Categories during seeding: {categoriesReply.Message()}" );
        (List<Category>, Dictionary<Guid, List<Category>>) sortedCategories = 
            SortCategories( categoriesReply.Enumerable.ToList() );
        _logger.LogInformation( "Seeded Categories." );
        
        // BRANDS
        (Replies<Brand>, Replies<BrandCategory>) brandsReply = await SeedBrands( sortedCategories.Item1 );
        if (!brandsReply.Item1.IsSuccess)
            throw new Exception( $"Failed to generate Brands during seeding: {brandsReply.Item1.Message()}" );
        if (!brandsReply.Item2.IsSuccess)
            throw new Exception( $"Failed to generate BrandCategories during seeding: {brandsReply.Item2.Message()}" );
        _logger.LogInformation( "Seeded Brands." );
        
        // PRODUCTS
        Reply<ProductSeedingModel> productsReply = 
            await SeedProducts( sortedCategories.Item1, sortedCategories.Item2, brandsReply.Item1.Enumerable.ToList(), brandsReply.Item2.Enumerable.ToList() );
        if (!productsReply.IsSuccess)
            throw new Exception( $"Failed to generate Products during seeding: {productsReply.Message()}" );
        _logger.LogInformation( "Seeded Products." );
    }

    async Task<Replies<Category>> SeedCategories()
    {
        const string tvpName = "CategoriesTvp";
        const string sql =
            """
            INSERT INTO Categories (Id, ParentId, Name)
            SELECT ID, ParentId, Name
            FROM @CategoriesTvp
            """;

        List<Category> categories = CategoryGenerator.GenerateCategories();
        DataTable tableParam = CategoryGenerator.GenerateCategoriesTable( categories );
        DynamicParameters parameters = new();
        parameters.Add( tvpName, tableParam.AsTableValuedParameter( tvpName ) );

        Reply<int> result = await _dapper.ExecuteAsync( sql, parameters );
        return result.IsSuccess && result.Data > 0
            ? Replies<Category>.With( categories )
            : Replies<Category>.None( result );
    }
    async Task<(Replies<Brand>, Replies<BrandCategory>)> SeedBrands( List<Category> categories )
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
            INSERT INTO BrandCategories (BrandId, CategoryId)
            SELECT BrandId, CategoryId)
            FROM @BrandCategoriesTvp;
            """;

        List<Brand> brands = BrandGenerator.GenerateBrands();
        List<BrandCategory> brandCategories = BrandGenerator.GenerateBrandCategories( brands, categories );

        DataTable brandsTable = BrandGenerator.GenerateBrandsTable( brands );
        DataTable brandCategoriesTable = BrandGenerator.GenerateBrandCategoriesTable( brandCategories );

        DynamicParameters brandsParameters = new();
        brandsParameters.Add( tvpBrands, brandsTable.AsTableValuedParameter( tvpBrands ) );
        DynamicParameters brandCategoriesParameters = new();
        brandCategoriesParameters.Add( tvpBrandCategories, brandCategoriesTable.AsTableValuedParameter( tvpBrandCategories ) );

        Reply<int> brandsReply = await _dapper.ExecuteAsync( brandsSql, brandsParameters );
        if (!brandsReply.IsSuccess || brandsReply.Data <= 0)
            return (Replies<Brand>.With( brands ), Replies<BrandCategory>.None());

        Reply<int> brandCategoriesReply = await _dapper.ExecuteAsync( brandCategorySql, brandCategoriesParameters );
        return brandCategoriesReply.IsSuccess && brandsReply.Data > 0
            ? (Replies<Brand>.With( brands ), Replies<BrandCategory>.With( brandCategories ))
            : (Replies<Brand>.With( brands ), Replies<BrandCategory>.None( brandCategoriesReply.Message() ));
    }
    async Task<Reply<ProductSeedingModel>> SeedProducts( List<Category> primaryCategories, Dictionary<Guid,List<Category>> secondaryCategories, List<Brand> brands, List<BrandCategory> brandCategories )
    {
        ProductSeedingModel seed = await Task.Run( () => 
            ProductGenerator.GenerateProducts( primaryCategories, secondaryCategories, brands, brandCategories, _random ) );

        Reply<int> productsReply = await InsertProducts( _dapper, seed.Products );
        if (!productsReply.IsSuccess)
            return Reply<ProductSeedingModel>.None( productsReply );

        Reply<int> productCategoriesReply = await InsertProductCategories( _dapper, seed.ProductCategories );
        if (!productCategoriesReply.IsSuccess)
            return Reply<ProductSeedingModel>.None( productCategoriesReply );

        Reply<int> productDescriptionsReply = await InsertProductDescriptions( _dapper, seed.ProductDescriptions );
        if (!productDescriptionsReply.IsSuccess)
            return Reply<ProductSeedingModel>.None( productDescriptionsReply );

        Reply<int> productXmlsReply = await InsertProductXmls( _dapper, seed.ProductXmls );
        if (!productXmlsReply.IsSuccess)
            return Reply<ProductSeedingModel>.None( productXmlsReply );
        
        return Reply<ProductSeedingModel>.With( seed );
    }
    
    static async Task<Reply<int>> InsertProducts( IDapperContext dapper, List<Product> products )
    {
        const string tvpName = "ProductsTvp";
        const string Sql =
            """
            INSERT INTO Products (Id, PrimaryCategoryId, BrandId, Rating, IsInStock, IsFeatured, Name, Image, Price, SalePrice )
            SELECT (Id, PrimaryCategoryId, BrandId, Rating, IsInStock, IsFeatured, Name, Image, Price, SalePrice )
            FROM @ProductsTvp
            """;
        
        int index = 0;
        while ( index < products.Count ) {
            DataTable tableParam = ProductGenerator.GenerateProductsTable( products.Slice( index, DatabaseInsertPageSize ) );
            DynamicParameters parameters = new();
            parameters.Add( tvpName, tableParam.AsTableValuedParameter( tvpName ) );

            Reply<int> result = await dapper.ExecuteAsync( Sql, parameters );
            if (!result.IsSuccess)
                return result;
            
            index += DatabaseInsertPageSize;
        }

        throw new Exception( "No products were found for product seeding." );
    }
    static async Task<Reply<int>> InsertProductCategories( IDapperContext dapper, List<ProductCategory> productCategories )
    {
        const string tvpName = "ProductsTvp";
        const string Sql =
            """
            INSERT INTO ProductCategories (Id, PrimaryCategoryId, BrandId, Rating, IsInStock, IsFeatured, Name, Image, Price, SalePrice )
            SELECT (Id, PrimaryCategoryId, BrandId, Rating, IsInStock, IsFeatured, Name, Image, Price, SalePrice )
            FROM @ProductsTvp
            """;

        int index = 0;
        while ( index < productCategories.Count ) {
            DataTable tableParam = ProductGenerator.GenerateProductCategoriesTable( productCategories.Slice( index, DatabaseInsertPageSize ) );
            DynamicParameters parameters = new();
            parameters.Add( tvpName, tableParam.AsTableValuedParameter( tvpName ) );

            Reply<int> result = await dapper.ExecuteAsync( Sql, parameters );
            if (!result.IsSuccess)
                return result;

            index += DatabaseInsertPageSize;
        }

        throw new Exception( "No product categories were found for product seeding." );
    }
    static async Task<Reply<int>> InsertProductDescriptions( IDapperContext dapper, List<ProductDescription> productDescriptions )
    {
        const string tvpName = "ProductDescriptionsTvp";
        const string Sql =
            """
            INSERT INTO ProductDescriptions (ProductId, Description)
            SELECT (ProductId, Description)
            FROM @ProductDescriptionsTvp
            """;

        int index = 0;
        while ( index < productDescriptions.Count ) {
            DataTable tableParam = ProductGenerator.GenerateProductDescriptionsTable( productDescriptions.Slice( index, DatabaseInsertPageSize ) );
            DynamicParameters parameters = new();
            parameters.Add( tvpName, tableParam.AsTableValuedParameter( tvpName ) );

            Reply<int> result = await dapper.ExecuteAsync( Sql, parameters );
            if (!result.IsSuccess)
                return result;

            index += DatabaseInsertPageSize;
        }

        throw new Exception( "No product descriptions were found for product seeding." );
    }
    static async Task<Reply<int>> InsertProductXmls( IDapperContext dapper, List<ProductXml> productXmls )
    {
        const string tvpName = "ProductXmlsTvp";
        const string Sql =
            """
            INSERT INTO ProductXmls (ProductId, Xml)
            SELECT (ProductId, Xml)
            FROM @ProductXmlsTvp
            """;

        int index = 0;
        while ( index < productXmls.Count ) {
            DataTable tableParam = ProductGenerator.GenerateProductXmlTable( productXmls.Slice( index, DatabaseInsertPageSizeBigData ) );
            DynamicParameters parameters = new();
            parameters.Add( tvpName, tableParam.AsTableValuedParameter( tvpName ) );

            Reply<int> result = await dapper.ExecuteAsync( Sql, parameters );
            if (!result.IsSuccess)
                return result;

            index += DatabaseInsertPageSize;
        }

        throw new Exception( "No product xmls were found for product seeding." );
    }

    static (List<Category>, Dictionary<Guid, List<Category>>) SortCategories( List<Category> categories )
    {
        List<Category> primaryCategories = categories.Where( c => c.ParentId is null ).ToList();
        Dictionary<Guid, List<Category>> secondaryCategories = [];
        foreach ( Category c in primaryCategories ) {
            if (c.ParentId is null) {
                secondaryCategories.TryAdd( c.Id, [] );
                continue;
            }

            if (!secondaryCategories.TryGetValue( c.ParentId.Value, out List<Category>? cs )) {
                cs = [];
                secondaryCategories.Add( c.ParentId.Value, cs );
            }

            cs.Add( c );
        }

        return (primaryCategories, secondaryCategories);
    }
}