using CatalogApplication.Database;
using CatalogApplication.Types.Brands.Models;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Filters.Models;
using CatalogApplication.Types.Products.Models;
using CatalogApplication.Types.ReplyTypes;

namespace CatalogApplication.Seed;

internal static class ProductSeeder
{
    internal static async Task<Reply<bool>> SeedProducts( IDapperContext dapper, List<Category> categories, List<Brand> brands, List<BrandCategory> brandCategories, RandomUtility random )
    {
        return Reply<bool>.None();
    }
    static List<Product> GenerateProducts( IDapperContext dapper, List<Category> categories, List<Brand> brands, List<BrandCategory> brandCategories, RandomUtility random )
    {
        List<Product> products = [];

        for ( int i = 0; i < 1000; i++ ) {
            Product p = new(
                Guid.NewGuid(),
                PickBrandId( brands, random ),
                PickRating( random ),
                PickIsInStock( random ),
                PickIsFeatured( random ),
                "",
                "",
                0,
                0 );
        }
        
        return products;
    }
    static Guid PickBrandId( List<Brand> brands, RandomUtility random )
    {
        int index = random.GetRandomInt( brands.Count - 1 );
        return brands[index].Id;
    }
    static float PickRating( RandomUtility random )
    {
        float value = (float) random.GetRandomDouble( Consts.MaxRating );
        return value;
    }
    static bool PickIsInStock( RandomUtility random )
    {
        bool value = random.GetRandomBool( 0.95 );
        return value;
    }
    static bool PickIsFeatured( RandomUtility random )
    {
        bool value = random.GetRandomBool( 0.2 );
        return value;
    }
}