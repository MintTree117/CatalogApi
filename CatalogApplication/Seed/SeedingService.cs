using System.Data;
using CatalogApplication.Database;
using CatalogApplication.Types.Brands.Models;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Filters.Models;
using CatalogApplication.Types.ReplyTypes;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Seed;

internal sealed class SeedingService
{
    readonly IServiceProvider _serviceProvider;
    readonly ILogger<SeedingService> _logger;
    static readonly RandomUtility _random = new();

    internal List<Category> CategoriesInMemory { get; private set; }
    internal List<Brand> BrandsInMemory { get; private set; }
    internal List<BrandCategory> BrandCategoriesInMemory { get; private set; }

    public SeedingService( IServiceProvider serviceProvider, ILogger<SeedingService> logger )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        SeedInMemory();
    }
    
    void SeedInMemory()
    {
        CategoriesInMemory = GetCategoriesInMemory();
        (List<Brand>, List<BrandCategory>) brands = GetBrandsInMemory();
        BrandsInMemory = brands.Item1;
        BrandCategoriesInMemory = brands.Item2;
    }
    async Task SeedDatabase()
    {
        // INIT
        IDapperContext dapper = IDapperContext.GetContext( _serviceProvider );
        await using SqlConnection connection = await dapper.GetOpenConnection();
        if (connection.State is not ConnectionState.Open)
            throw new Exception( "Invalid ConnectionState while seeding the database." );
        
        // CATEGORIES
        Replies<Category> categoriesReply = await CategorySeeder.SeedCategories( dapper, _random );
        if (!categoriesReply.IsSuccess)
            throw new Exception( $"Seed Categories Error: {categoriesReply.Message()}" );
        
        // BRANDS
        (Replies<Brand>, Replies<BrandCategory>) brandsReply = await BrandSeeder.SeedBrands( dapper, categoriesReply.Enumerable.ToList(), _random );
        if (!brandsReply.Item1.IsSuccess)
            throw new Exception( $"Seed Brands Error: {brandsReply.Item1.Message()}" );
        if (!brandsReply.Item2.IsSuccess)
            throw new Exception( $"Seed BrandCategories Error: {brandsReply.Item2.Message()}" );
    }

    List<Category> GetCategoriesInMemory()
    {
        Replies<Category> result = CategorySeeder.SeedCategoriesInMemory( _random );
        if (result.IsSuccess)
            CategoriesInMemory = result.Enumerable.ToList();
        return result.IsSuccess
            ? result.Enumerable.ToList()
            : [];
    }
    (List<Brand>, List<BrandCategory>) GetBrandsInMemory()
    {
        (Replies<Brand>, Replies<BrandCategory>) result = BrandSeeder.SeedBrandsInMemory( CategoriesInMemory ?? [], _random );
        return result.Item1.IsSuccess && result.Item2.IsSuccess
            ? (result.Item1.Enumerable.ToList(), result.Item2.Enumerable.ToList())
            : ([], []);
    }
}