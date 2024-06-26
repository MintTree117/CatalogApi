using CatalogApplication.Database;
using CatalogApplication.Types._Common.ReplyTypes;
using CatalogApplication.Types.Products.Dtos;
using CatalogApplication.Types.Products.Models;

namespace CatalogApplication.Repositories;

internal sealed class ProductSpecialRepository( IDapperContext dapper, ILogger<ProductSearchRepository> logger )
    : BaseRepository<ProductSearchRepository>( dapper, logger )
{
    FrontPageProductsDto _frontPageCache;

    internal async Task<Replies<Product>> GetFrontPageSpecials()
    {
        throw new Exception();
    }
    internal async Task<Replies<Product>> GetProductsForIds( List<Guid> productIds )
    {
        throw new Exception();
    }
    
    async Task<Replies<Product>> GetTopFeatured()
    {
        throw new Exception();
    }
    async Task<Replies<Product>> GetTopSales()
    {
        throw new Exception();
    }
    async Task<Replies<Product>> GetTopSelling()
    {
        throw new Exception();
    }
}