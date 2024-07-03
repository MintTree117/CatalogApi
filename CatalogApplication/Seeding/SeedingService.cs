using CatalogApplication.Database;
using CatalogApplication.Seeding.Generators;
using CatalogApplication.Types._Common.ReplyTypes;
using CatalogApplication.Types.Brands.Models;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Products.Models;
using CatalogApplication.Types.Warehouses;
using Dapper;

namespace CatalogApplication.Seeding;

internal sealed class SeedingService( IDapperContext dapper, ILogger<SeedingService> logger )
{
    const int DatabaseInsertPageSize = 100;
    const int DatabaseInsertPageSizeBigData = 25;
    
    readonly IDapperContext _dapper = dapper;
    readonly ILogger<SeedingService> _logger = logger;
    static readonly RandomUtility _random = new();

    internal async Task SeedDatabase()
    {
        // CLEAR CATALOG
        var clearReply = await _dapper.ExecuteStoredProcedure( "CatalogApi.ClearCatalog" );
        if (!clearReply)
            _logger.LogWarning( $"Failed to clear database during seeding: {clearReply.GetMessage()}" );
        else
            _logger.LogInformation( "Cleared Catalog." );
        
        // CATEGORIES
        var categoriesReply = await SeedCategories();
        if (!categoriesReply)
            throw new Exception( $"Failed to seed Categories during seeding: {categoriesReply.GetMessage()}" );
        (List<Category>, Dictionary<Guid, List<Category>>) sortedCategories = 
            SortCategories( categoriesReply.Enumerable.ToList() );
        
        _logger.LogInformation( "----------------------------------------------------------------------------------------------" );
        _logger.LogInformation( "SEEDED CATEGORIES" );
        _logger.LogInformation( "----------------------------------------------------------------------------------------------" );
        _logger.LogInformation( $"Primary Count: {sortedCategories.Item1.Count}" );
        _logger.LogInformation( $"Secondary Count: {sortedCategories.Item2.Count}" );
        
        // BRANDS
        (Replies<Brand>, Replies<BrandCategory>) brandsReply = await SeedBrands( sortedCategories.Item1 );
        if (!brandsReply.Item1)
            throw new Exception( $"Failed to seed Brands during seeding: {brandsReply.Item1.GetMessage()}" );
        if (!brandsReply.Item2)
            throw new Exception( $"Failed to seed BrandCategories during seeding: {brandsReply.Item2.GetMessage()}" );

        _logger.LogInformation( "----------------------------------------------------------------------------------------------" );
        _logger.LogInformation( "SEEDED BRANDS" );
        _logger.LogInformation( "----------------------------------------------------------------------------------------------" );
        _logger.LogInformation( $"Brands Count: {brandsReply.Item1.Enumerable.Count()}" );
        _logger.LogInformation( $"BrandCategories Count: {brandsReply.Item2.Enumerable.Count()}" );
        
        // PRODUCTS
        var productsReply = 
            await SeedProducts( sortedCategories.Item1, sortedCategories.Item2, brandsReply.Item1.Enumerable.ToList(), brandsReply.Item2.Enumerable.ToList() );
        if (!productsReply)
            throw new Exception( $"Failed to seed Products during seeding: {productsReply.GetMessage()}" );

        _logger.LogInformation( "----------------------------------------------------------------------------------------------" );
        _logger.LogInformation( "SEEDED PRODUCTS" );
        _logger.LogInformation( "----------------------------------------------------------------------------------------------" );
        _logger.LogInformation( $"Products Count: {productsReply.Data.Products.Count}" );
        _logger.LogInformation( $"ProductCategories Count: {productsReply.Data.ProductCategories.Count}" );
        _logger.LogInformation( $"ProductDescriptions Count: {productsReply.Data.ProductDescriptions.Count}" );
        _logger.LogInformation( $"ProductXmls Count: {productsReply.Data.ProductXmls.Count}" );
        
        // WAREHOUSES
        var warehousesReply = await SeedWarehouses();
        if (!warehousesReply)
            throw new Exception( $"Failed to seed Warehouses during seeding: {warehousesReply.GetMessage()}" );
        _logger.LogInformation( "Seeded Warehouses." );
        
        // INVENTORIES
        var inventoriesReply = await SeedInventory( productsReply.Data.Products, warehousesReply.Enumerable.ToList() );
        if (!inventoriesReply)
            throw new Exception( $"Failed to seed Inventories during seeding: {inventoriesReply.GetMessage()}" );
        _logger.LogInformation( "Seeded Inventories." );
    }

    async Task<Replies<Category>> SeedCategories()
    {
        const string sql =
            """
            INSERT INTO CatalogApi.Categories (Id, ParentId, [Name])
            SELECT Id, ParentId, [Name]
            FROM @CategoriesTvp
            """;

        var categories = CategoryGenerator.GenerateCategories();
        var tableParam = CategoryGenerator.GenerateCategoriesTable( categories );
        var parameters = new DynamicParameters();
        parameters.Add( "CategoriesTvp", tableParam.AsTableValuedParameter( "CatalogApi.CategoriesTvp" ) );

        var result = await _dapper.ExecuteAsync( sql, parameters );
        return result && result.Data > 0
            ? Replies<Category>.Success( categories )
            : Replies<Category>.Fail( result );
    }
    async Task<(Replies<Brand>, Replies<BrandCategory>)> SeedBrands( List<Category> categories )
    {
        const string brandsSql =
            """
            INSERT INTO CatalogApi.Brands (Id, [Name])
            SELECT Id, [Name]
            FROM @BrandsTvp;
            """;
        const string brandCategoriesSql =
            """
            INSERT INTO CatalogApi.BrandCategories (BrandId, CategoryId)
            SELECT BrandId, CategoryId
            FROM @BrandCategoriesTvp;
            """;

        var brands = BrandGenerator.GenerateBrands();
        var brandCategories = BrandGenerator.GenerateBrandCategories( brands, categories );

        using var brandsTable = BrandGenerator.GenerateBrandsTable( brands );
        using var brandCategoriesTable = BrandGenerator.GenerateBrandCategoriesTable( brandCategories );

        var brandsParameters = new DynamicParameters();
        var brandCategoriesParameters = new DynamicParameters();
        brandsParameters.Add( "BrandsTvp", brandsTable.AsTableValuedParameter( "CatalogApi.BrandsTvp" ) );
        brandCategoriesParameters.Add( "BrandCategoriesTvp", brandCategoriesTable.AsTableValuedParameter( "CatalogApi.BrandCategoriesTvp" ) );

        var brandsReply = await _dapper.ExecuteAsync( brandsSql, brandsParameters );
        if (!brandsReply || brandsReply.Data <= 0)
            return (Replies<Brand>.Success( brands ), Replies<BrandCategory>.Fail());

        var brandCategoriesReply = await _dapper.ExecuteAsync( brandCategoriesSql, brandCategoriesParameters );
        return brandCategoriesReply && brandsReply.Data > 0
            ? (Replies<Brand>.Success( brands ), Replies<BrandCategory>.Success( brandCategories ))
            : (Replies<Brand>.Success( brands ), Replies<BrandCategory>.Fail( brandCategoriesReply.GetMessage() ));
    }
    async Task<Reply<ProductSeedingModel>> SeedProducts( List<Category> primaryCategories, Dictionary<Guid,List<Category>> secondaryCategories, List<Brand> brands, List<BrandCategory> brandCategories )
    {
        ProductSeedingModel seed = await Task.Run( () => 
            ProductGenerator.GenerateProducts( primaryCategories, secondaryCategories, brands, brandCategories, _random ) );

        var productsReply = await InsertProducts( _dapper, seed.Products );
        if (!productsReply)
        {
            _logger.LogError( $"jjFailed to generate products {productsReply.GetMessage()}" );
            return Reply<ProductSeedingModel>.Failure( productsReply );
        }


        var productCategoriesReply = await InsertProductCategories( _dapper, seed.ProductCategories );
        if (!productCategoriesReply)
            return Reply<ProductSeedingModel>.Failure( productCategoriesReply );

        var productDescriptionsReply = await InsertProductDescriptions( _dapper, seed.ProductDescriptions );
        if (!productDescriptionsReply)
            return Reply<ProductSeedingModel>.Failure( productDescriptionsReply );

        var productXmlsReply = await InsertProductXmls( _dapper, seed.ProductXmls );
        if (!productXmlsReply)
            return Reply<ProductSeedingModel>.Failure( productXmlsReply );
        
        return Reply<ProductSeedingModel>.Success( seed );
    }
    async Task<Replies<Warehouse>> SeedWarehouses()
    {
        const string Sql =
            """
            INSERT INTO CatalogApi.Warehouses (Id, QueryUrl, PosX, PosY)
            SELECT Id, QueryUrl, PosX, PosY
            FROM @WarehousesTvp
            """;

        var warehouses = WarehouseGenerator.GenerateWarehouses();
        using var tableParam = WarehouseGenerator.GenerateWarehousesTable( warehouses );
        var parameters = new DynamicParameters();
        parameters.Add( "WarehousesTvp", tableParam.AsTableValuedParameter( "CatalogApi.WarehousesTvp" ) );

        var reply = await _dapper.ExecuteAsync( Sql, parameters );
        return reply
            ? Replies<Warehouse>.Success( warehouses )
            : Replies<Warehouse>.Fail( reply );
    }
    async Task<Reply<bool>> SeedInventory( List<Product> products, List<Warehouse> warehouses )
    {
        var inventories = InventoryGenerator.GenerateInventories( products, warehouses, _random );
        using var tableParam = InventoryGenerator.GenerateInventoryTable( inventories );

        var parameters = new DynamicParameters();
        parameters.Add( "ProductInventoriesTvp", tableParam.AsTableValuedParameter( "CatalogApi.ProductInventoriesTvp" ) );

        var reply = await InsertInventories( _dapper, inventories );
        return reply
            ? Reply<bool>.Success( true )
            : Reply<bool>.Failure( reply );
    }

    async Task<Reply<bool>> InsertInventories( IDapperContext dapper, List<ProductInventory> inventories )
    {
        const string sql =
            """
            INSERT INTO CatalogApi.ProductInventories (ProductId, WarehouseId, Quantity)
            SELECT ProductId, WarehouseId, Quantity
            FROM @ProductInventoriesTvp
            """;

        int index = 0;
        while ( index < inventories.Count )
        {
            var productsSubset = inventories.Skip( index ).Take( DatabaseInsertPageSize ).ToList();
            using var tableParam = InventoryGenerator.GenerateInventoryTable( productsSubset );
            var parameters = new DynamicParameters();
            parameters.Add( "ProductInventoriesTvp", tableParam.AsTableValuedParameter( "CatalogApi.ProductInventoriesTvp" ) );

            var result = await dapper.ExecuteAsync( sql, parameters );
            if (!result)
                return Reply<bool>.Failure( result );

            index += DatabaseInsertPageSize;
        }

        return Reply<bool>.Success( true );
    }
    async Task<Reply<bool>> InsertProducts( IDapperContext dapper, List<Product> products )
    {
        const string sql =
            """
            INSERT INTO CatalogApi.Products (Id, BrandId, [Name], BrandName, Image, IsFeatured, IsInStock, Price, SalePrice, Rating, NumberRatings, NumberSold  )
            SELECT Id, BrandId, [Name], BrandName, Image, IsFeatured, IsInStock, Price, SalePrice, Rating, NumberRatings, NumberSold
            FROM @ProductsTvp
            """;
        
        int index = 0;
        while ( index < products.Count )
        {
            var productsSubset = products.Skip( index ).Take( DatabaseInsertPageSize ).ToList();
            using var tableParam = ProductGenerator.GenerateProductsTable( productsSubset );
            var parameters = new DynamicParameters();
            parameters.Add( "ProductsTvp", tableParam.AsTableValuedParameter( "CatalogApi.ProductsTvp" ) );

            var result = await dapper.ExecuteAsync( sql, parameters );
            if (!result)
            {
                _logger.LogError( $"Error during insert products {result.GetMessage()}" );
                return Reply<bool>.Failure( result );
            }

            index += DatabaseInsertPageSize;
        }

        return Reply<bool>.Success( true );
    }
    async Task<Reply<bool>> InsertProductCategories( IDapperContext dapper, List<ProductCategory> productCategories )
    {
        const string Sql =
            """
            INSERT INTO CatalogApi.ProductCategories (ProductId, CategoryId)
            SELECT ProductId, CategoryId
            FROM @ProductCategoriesTvp
            """;

        int index = 0;
        while ( index < productCategories.Count ) {
            var categoriesSubset = productCategories.Skip( index ).Take( DatabaseInsertPageSize ).ToList();
            var tableParam = ProductGenerator.GenerateProductCategoriesTable( categoriesSubset );
            var parameters = new DynamicParameters();
            parameters.Add( "ProductCategoriesTvp", tableParam.AsTableValuedParameter( "CatalogApi.ProductCategoriesTvp" ) );

            var result = await dapper.ExecuteAsync( Sql, parameters );
            if (!result)
                return Reply<bool>.Failure( result );

            index += DatabaseInsertPageSize;
        }

        return Reply<bool>.Success( true );
    }
    async Task<Reply<bool>> InsertProductDescriptions( IDapperContext dapper, List<ProductDescription> productDescriptions )
    {
        const string Sql =
            """
            INSERT INTO CatalogApi.ProductDescriptions (ProductId, Description)
            SELECT ProductId, Description
            FROM @ProductDescriptionsTvp
            """;

        int index = 0;
        while ( index < productDescriptions.Count ) {
            var descriptionsSubset = productDescriptions.Skip( index ).Take( DatabaseInsertPageSize ).ToList();
            using var tableParam = ProductGenerator.GenerateProductDescriptionsTable( descriptionsSubset );
            var parameters = new DynamicParameters();
            parameters.Add( "ProductDescriptionsTvp", tableParam.AsTableValuedParameter( "CatalogApi.ProductDescriptionsTvp" ) );

            var result = await dapper.ExecuteAsync( Sql, parameters );
            if (!result)
                return Reply<bool>.Failure( result );

            index += DatabaseInsertPageSize;
        }

        return Reply<bool>.Success( true );
    }
    async Task<Reply<bool>> InsertProductXmls( IDapperContext dapper, List<ProductXml> productXmls )
    {
        const string Sql =
            """
            INSERT INTO CatalogApi.ProductXmls (ProductId, Xml)
            SELECT ProductId, Xml
            FROM @ProductXmlsTvp
            """;

        int index = 0;
        while ( index < productXmls.Count ) {
            var xmlsSubset = productXmls.Skip( index ).Take( DatabaseInsertPageSize ).ToList();
            var tableParam = ProductGenerator.GenerateProductXmlTable( xmlsSubset );
            var parameters = new DynamicParameters();
            parameters.Add( "ProductXmlsTvp", tableParam.AsTableValuedParameter( "CatalogApi.ProductXmlsTvp" ) );

            var result = await dapper.ExecuteAsync( Sql, parameters );
            if (!result)
                return Reply<bool>.Failure( result );

            index += DatabaseInsertPageSize;
        }

        return Reply<bool>.Success( true );
    }

    static (List<Category>, Dictionary<Guid, List<Category>>) SortCategories( List<Category> categories )
    {
        var primaryCategories = categories.Where( static c => c.ParentId is null ).ToList();
        
        Dictionary<Guid, List<Category>> secondaryCategories = [];
        foreach ( Category c in categories ) {
            if (c.ParentId is null) {
                if (!secondaryCategories.ContainsKey( c.Id ))
                    secondaryCategories.Add( c.Id, [] );
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