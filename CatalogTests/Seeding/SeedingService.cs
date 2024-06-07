using System.Data;
using CatalogApplication.Database;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Filters.Models;
using CatalogApplication.Types.ReplyTypes;
using CatalogTests.Seeding.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace CatalogTests.Seeding;

public sealed class SeedingService( IServiceProvider serviceProvider, ILogger<SeedingService> logger )
{
    readonly IServiceProvider _serviceProvider = serviceProvider;
    readonly ILogger<SeedingService> _logger = logger;
    static readonly RandomUtility _random = new();

    async Task SeedDatabase()
    {
        // INIT
        IDapperContext dapper = IDapperContext.GetContext( _serviceProvider );
        await using SqlConnection connection = await dapper.GetOpenConnection();
        if (connection.State is not ConnectionState.Open)
            throw new Exception( "Invalid ConnectionState while seeding the database." );
        
        // CATEGORIES
        Replies<Category> categoriesReply = await CategorySeedUtils.SeedCategories( dapper, _random );
        if (!categoriesReply.IsSuccess)
            throw new Exception( $"Seed Categories Error: {categoriesReply.Message()}" );
        
        // BRANDS
        (Replies<Brand>, Replies<BrandCategory>) brandsReply = await BrandSeedUtils.SeedBrands( dapper, categoriesReply.Enumerable.ToList(), _random );
        if (!brandsReply.Item1.IsSuccess)
            throw new Exception( $"Seed Brands Error: {brandsReply.Item1.Message()}" );
        if (!brandsReply.Item2.IsSuccess)
            throw new Exception( $"Seed BrandCategories Error: {brandsReply.Item2.Message()}" );
        
    }
}