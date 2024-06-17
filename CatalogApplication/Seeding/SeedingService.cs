using System.Data;
using CatalogApplication.Database;
using CatalogApplication.Seeding.Generators;
using CatalogApplication.Types._Common.ReplyTypes;
using CatalogApplication.Types.Brands.Models;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Products.Models;
using CatalogApplication.Types.Stock;
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
        Reply<int> clearReply = await _dapper.ExecuteStoredProcedure( "CatalogApi.ClearCatalog" );
        if (!clearReply.IsSuccess)
            _logger.LogWarning( $"Failed to clear database during seeding: {clearReply.Message()}" );
        else
            _logger.LogInformation( "Cleared Catalog." );
        
        // CATEGORIES
        Replies<Category> categoriesReply = await SeedCategories();
        if (!categoriesReply.IsSuccess)
            throw new Exception( $"Failed to seed Categories during seeding: {categoriesReply.Message()}" );
        (List<Category>, Dictionary<Guid, List<Category>>) sortedCategories = 
            SortCategories( categoriesReply.Enumerable.ToList() );
        
        _logger.LogInformation( "----------------------------------------------------------------------------------------------" );
        _logger.LogInformation( "SEEDED CATEGORIES" );
        _logger.LogInformation( "----------------------------------------------------------------------------------------------" );
        _logger.LogInformation( $"Primary Count: {sortedCategories.Item1.Count}" );
        _logger.LogInformation( $"Secondary Count: {sortedCategories.Item2.Count}" );
        
        // BRANDS
        (Replies<Brand>, Replies<BrandCategory>) brandsReply = await SeedBrands( sortedCategories.Item1 );
        if (!brandsReply.Item1.IsSuccess)
            throw new Exception( $"Failed to seed Brands during seeding: {brandsReply.Item1.Message()}" );
        if (!brandsReply.Item2.IsSuccess)
            throw new Exception( $"Failed to seed BrandCategories during seeding: {brandsReply.Item2.Message()}" );

        _logger.LogInformation( "----------------------------------------------------------------------------------------------" );
        _logger.LogInformation( "SEEDED BRANDS" );
        _logger.LogInformation( "----------------------------------------------------------------------------------------------" );
        _logger.LogInformation( $"Brands Count: {brandsReply.Item1.Enumerable.Count()}" );
        _logger.LogInformation( $"BrandCategories Count: {brandsReply.Item2.Enumerable.Count()}" );
        
        // PRODUCTS
        Reply<ProductSeedingModel> productsReply = 
            await SeedProducts( sortedCategories.Item1, sortedCategories.Item2, brandsReply.Item1.Enumerable.ToList(), brandsReply.Item2.Enumerable.ToList() );
        if (!productsReply.IsSuccess)
            throw new Exception( $"Failed to seed Products during seeding: {productsReply.Message()}" );

        _logger.LogInformation( "----------------------------------------------------------------------------------------------" );
        _logger.LogInformation( "SEEDED PRODUCTS" );
        _logger.LogInformation( "----------------------------------------------------------------------------------------------" );
        _logger.LogInformation( $"Products Count: {productsReply.Data.Products.Count}" );
        _logger.LogInformation( $"ProductCategories Count: {productsReply.Data.ProductCategories.Count}" );
        _logger.LogInformation( $"ProductDescriptions Count: {productsReply.Data.ProductDescriptions.Count}" );
        _logger.LogInformation( $"ProductXmls Count: {productsReply.Data.ProductXmls.Count}" );
        
        // WAREHOUSES
        Replies<Warehouse> warehousesReply = await SeedWarehouses();
        if (!warehousesReply.IsSuccess)
            throw new Exception( $"Failed to seed Warehouses during seeding: {warehousesReply.Message()}" );
        _logger.LogInformation( "Seeded Warehouses." );
        
        // INVENTORIES
        Reply<bool> inventoriesReply = await SeedInventory( productsReply.Data.Products, warehousesReply.Enumerable.ToList() );
        if (!inventoriesReply.IsSuccess)
            throw new Exception( $"Failed to seed Inventories during seeding: {inventoriesReply.Message()}" );
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

        List<Category> categories = CategoryGenerator.GenerateCategories();
        DataTable tableParam = CategoryGenerator.GenerateCategoriesTable( categories );
        DynamicParameters parameters = new();
        parameters.Add( "CategoriesTvp", tableParam.AsTableValuedParameter( "CatalogApi.CategoriesTvp" ) );

        Reply<int> result = await _dapper.ExecuteAsync( sql, parameters );
        return result.IsSuccess && result.Data > 0
            ? Replies<Category>.With( categories )
            : Replies<Category>.None( result );
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

        List<Brand> brands = BrandGenerator.GenerateBrands();
        List<BrandCategory> brandCategories = BrandGenerator.GenerateBrandCategories( brands, categories );

        DataTable brandsTable = BrandGenerator.GenerateBrandsTable( brands );
        DataTable brandCategoriesTable = BrandGenerator.GenerateBrandCategoriesTable( brandCategories );

        DynamicParameters brandsParameters = new();
        brandsParameters.Add( "BrandsTvp", brandsTable.AsTableValuedParameter( "CatalogApi.BrandsTvp" ) );
        DynamicParameters brandCategoriesParameters = new();
        brandCategoriesParameters.Add( "BrandCategoriesTvp", brandCategoriesTable.AsTableValuedParameter( "CatalogApi.BrandCategoriesTvp" ) );

        Reply<int> brandsReply = await _dapper.ExecuteAsync( brandsSql, brandsParameters );
        if (!brandsReply.IsSuccess || brandsReply.Data <= 0)
            return (Replies<Brand>.With( brands ), Replies<BrandCategory>.None());

        Reply<int> brandCategoriesReply = await _dapper.ExecuteAsync( brandCategoriesSql, brandCategoriesParameters );
        return brandCategoriesReply.IsSuccess && brandsReply.Data > 0
            ? (Replies<Brand>.With( brands ), Replies<BrandCategory>.With( brandCategories ))
            : (Replies<Brand>.With( brands ), Replies<BrandCategory>.None( brandCategoriesReply.Message() ));
    }
    async Task<Reply<ProductSeedingModel>> SeedProducts( List<Category> primaryCategories, Dictionary<Guid,List<Category>> secondaryCategories, List<Brand> brands, List<BrandCategory> brandCategories )
    {
        ProductSeedingModel seed = await Task.Run( () => 
            ProductGenerator.GenerateProducts( primaryCategories, secondaryCategories, brands, brandCategories, _random ) );

        Reply<bool> productsReply = await InsertProducts( _dapper, seed.Products );
        if (!productsReply.IsSuccess)
            return Reply<ProductSeedingModel>.None( productsReply );

        Reply<bool> productCategoriesReply = await InsertProductCategories( _dapper, seed.ProductCategories );
        if (!productCategoriesReply.IsSuccess)
            return Reply<ProductSeedingModel>.None( productCategoriesReply );

        Reply<bool> productDescriptionsReply = await InsertProductDescriptions( _dapper, seed.ProductDescriptions );
        if (!productDescriptionsReply.IsSuccess)
            return Reply<ProductSeedingModel>.None( productDescriptionsReply );

        Reply<bool> productXmlsReply = await InsertProductXmls( _dapper, seed.ProductXmls );
        if (!productXmlsReply.IsSuccess)
            return Reply<ProductSeedingModel>.None( productXmlsReply );
        
        return Reply<ProductSeedingModel>.With( seed );
    }
    async Task<Replies<Warehouse>> SeedWarehouses()
    {
        const string Sql =
            """
            INSERT INTO CatalogApi.Warehouses (Id, QueryUrl, PosX, PosY)
            SELECT Id, QueryUrl, PosX, PosY
            FROM @WarehousesTvp
            """;

        List<Warehouse> warehouses = WarehouseGenerator.GenerateWarehouses();
        DataTable tableParam = WarehouseGenerator.GenerateWarehousesTable( warehouses );
        DynamicParameters parameters = new();
        parameters.Add( "WarehousesTvp", tableParam.AsTableValuedParameter( "CatalogApi.WarehousesTvp" ) );

        Reply<int> reply = await _dapper.ExecuteAsync( Sql, parameters );
        return reply.IsSuccess
            ? Replies<Warehouse>.With( warehouses )
            : Replies<Warehouse>.None( reply );
    }
    async Task<Reply<bool>> SeedInventory( List<Product> products, List<Warehouse> warehouses )
    {
        List<ProductInventory> inventories = InventoryGenerator.GenerateInventories( products, warehouses, _random );
        DataTable tableParam = InventoryGenerator.GenerateInventoryTable( inventories );

        DynamicParameters parameters = new();
        parameters.Add( "ProductInventoriesTvp", tableParam.AsTableValuedParameter( "CatalogApi.ProductInventoriesTvp" ) );

        Reply<bool> reply = await InsertInventories( _dapper, inventories );
        return reply.IsSuccess
            ? Reply<bool>.With( true )
            : Reply<bool>.None( reply );
    }

    static async Task<Reply<bool>> InsertInventories( IDapperContext dapper, List<ProductInventory> inventories )
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
            List<ProductInventory> productsSubset = inventories.Skip( index ).Take( DatabaseInsertPageSize ).ToList();
            DataTable tableParam = InventoryGenerator.GenerateInventoryTable( productsSubset );
            DynamicParameters parameters = new();
            parameters.Add( "ProductInventoriesTvp", tableParam.AsTableValuedParameter( "CatalogApi.ProductInventoriesTvp" ) );

            Reply<int> result = await dapper.ExecuteAsync( sql, parameters );
            if (!result.IsSuccess)
                return Reply<bool>.None( result );

            index += DatabaseInsertPageSize;
        }

        return Reply<bool>.With( true );
    }
    
    static async Task<Reply<bool>> InsertProducts( IDapperContext dapper, List<Product> products )
    {
        const string sql =
            """
            INSERT INTO CatalogApi.Products (Id, PrimaryCategoryId, BrandId, IsInStock, IsFeatured, [Name], Image, Price, SalePrice )
            SELECT Id, PrimaryCategoryId, BrandId, IsInStock, IsFeatured, [Name], Image, Price, SalePrice
            FROM @ProductsTvp
            """;
        
        int index = 0;
        while ( index < products.Count ) {
            List<Product> productsSubset = products.Skip( index ).Take( DatabaseInsertPageSize ).ToList();
            DataTable tableParam = ProductGenerator.GenerateProductsTable( productsSubset );
            DynamicParameters parameters = new();
            parameters.Add( "ProductsTvp", tableParam.AsTableValuedParameter( "CatalogApi.ProductsTvp" ) );

            Reply<int> result = await dapper.ExecuteAsync( sql, parameters );
            if (!result.IsSuccess)
                return Reply<bool>.None( result );

            index += DatabaseInsertPageSize;
        }

        return Reply<bool>.With( true );
    }
    static async Task<Reply<bool>> InsertProductCategories( IDapperContext dapper, List<ProductCategory> productCategories )
    {
        const string Sql =
            """
            INSERT INTO CatalogApi.ProductCategories (ProductId, CategoryId)
            SELECT ProductId, CategoryId
            FROM @ProductCategoriesTvp
            """;

        int index = 0;
        while ( index < productCategories.Count ) {
            List<ProductCategory> categoriesSubset = productCategories.Skip( index ).Take( DatabaseInsertPageSize ).ToList();
            DataTable tableParam = ProductGenerator.GenerateProductCategoriesTable( categoriesSubset );
            DynamicParameters parameters = new();
            parameters.Add( "ProductCategoriesTvp", tableParam.AsTableValuedParameter( "CatalogApi.ProductCategoriesTvp" ) );

            Reply<int> result = await dapper.ExecuteAsync( Sql, parameters );
            if (!result.IsSuccess)
                return Reply<bool>.None( result );

            index += DatabaseInsertPageSize;
        }

        return Reply<bool>.With( true );
    }
    static async Task<Reply<bool>> InsertProductDescriptions( IDapperContext dapper, List<ProductDescription> productDescriptions )
    {
        const string Sql =
            """
            INSERT INTO CatalogApi.ProductDescriptions (ProductId, Description)
            SELECT ProductId, Description
            FROM @ProductDescriptionsTvp
            """;

        int index = 0;
        while ( index < productDescriptions.Count ) {
            List<ProductDescription> descriptionsSubset = productDescriptions.Skip( index ).Take( DatabaseInsertPageSize ).ToList();
            DataTable tableParam = ProductGenerator.GenerateProductDescriptionsTable( descriptionsSubset );
            DynamicParameters parameters = new();
            parameters.Add( "ProductDescriptionsTvp", tableParam.AsTableValuedParameter( "CatalogApi.ProductDescriptionsTvp" ) );

            Reply<int> result = await dapper.ExecuteAsync( Sql, parameters );
            if (!result.IsSuccess)
                return Reply<bool>.None( result );

            index += DatabaseInsertPageSize;
        }

        return Reply<bool>.With( true );
    }
    static async Task<Reply<bool>> InsertProductXmls( IDapperContext dapper, List<ProductXml> productXmls )
    {
        const string Sql =
            """
            INSERT INTO CatalogApi.ProductXmls (ProductId, Xml)
            SELECT ProductId, Xml
            FROM @ProductXmlsTvp
            """;

        int index = 0;
        while ( index < productXmls.Count ) {
            List<ProductXml> xmlsSubset = productXmls.Skip( index ).Take( DatabaseInsertPageSize ).ToList();
            DataTable tableParam = ProductGenerator.GenerateProductXmlTable( xmlsSubset );
            DynamicParameters parameters = new();
            parameters.Add( "ProductXmlsTvp", tableParam.AsTableValuedParameter( "CatalogApi.ProductXmlsTvp" ) );

            Reply<int> result = await dapper.ExecuteAsync( Sql, parameters );
            if (!result.IsSuccess)
                return Reply<bool>.None( result );

            index += DatabaseInsertPageSize;
        }

        return Reply<bool>.With( true );
    }

    static (List<Category>, Dictionary<Guid, List<Category>>) SortCategories( List<Category> categories )
    {
        List<Category> primaryCategories = categories.Where( static c => c.ParentId is null ).ToList();
        
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