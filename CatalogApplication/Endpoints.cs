using CatalogApplication.Repositories;
using CatalogApplication.Types._Common.Geography;
using CatalogApplication.Types.Brands.Dtos;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Products.Dtos;
using CatalogApplication.Types.Search.Dtos;
using CatalogApplication.Types.Search.Local;
using CatalogApplication.Utilities;

namespace CatalogApplication;

internal static class Endpoints
{
    internal static void MapEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapGet( "api/categories", static async ( CategoryRepository repository ) => 
            await GetCategories( repository ) );
        app.MapGet( "api/brands", static async ( BrandRepository repository ) =>
            await GetBrands( repository ) );
        app.MapGet( "api/search", static async ( HttpContext http, ProductSearchRepository products, InventoryRepository inventory ) =>
            await GetSearch( http, products, inventory ) );
        app.MapGet( "api/estimates", static async ( HttpContext http, InventoryRepository inventory ) =>
            await GetEstimates( http, inventory ) );
        app.MapGet( "api/details", static async ( HttpContext http, ProductDetailsRepository details, InventoryRepository inventory ) =>
            await GetDetails( http, details, inventory ) );
    }
    
    static async Task<IResult> GetCategories( CategoryRepository repository )
    {
        List<Category> result = (await repository.GetCategories()).ToList();
        return result.Count > 0
            ? Results.Ok( result )
            : Results.NotFound();
    }
    static async Task<IResult> GetBrands( BrandRepository repository )
    {
        BrandsReply? result = await repository.GetBrands();
        return result is not null
            ? Results.Ok( result )
            : Results.NotFound();
    }
    static async Task<IResult> GetSearch( HttpContext http, ProductSearchRepository products, InventoryRepository inventory )
    {
        // FILTERS
        IQueryCollection query = http.Request.Query;
        SearchFilters filters = new(
            query["SearchText"],
            Utils.ParseGuid( query["CategoryId"] ),
            Utils.ParseGuidList( query["BrandIds"] ),
            Utils.ParseInt( query["MinPrice"] ),
            Utils.ParseInt( query["MaxPrice"] ),
            Utils.ParseBool( query["IsInStock"] ),
            Utils.ParseBool( query["IsFeatured"] ),
            Utils.ParseBool( query["IsOnSale"] ) ?? false,
            Utils.ParseInt( query["Page"] ) ?? 0,
            Utils.ParseInt( query["PageSize"] ) ?? 5,
            Utils.ParseInt( query["SortBy"] ) ?? 0,
            Utils.ParseInt( query["PosX"] ),
            Utils.ParseInt( query["PosY"] )
        );
        
        // SEARCH
        SearchQueryReply? searchReply = await products.GetSearch( filters );
        if (searchReply is null)
            return Results.NotFound();
        
        // SHIPPING
        AddressDto? deliveryAddress = filters.PosX is null || filters.PosY is null 
            ? null : new AddressDto( filters.PosX.Value, filters.PosY.Value );
        List<Guid> productIds = searchReply.Value.Results.Select( static p => p.Id ).ToList();
        List<int> estimatesReply = await inventory.GetDeliveryEstimates( productIds, deliveryAddress );
        
        // FINISH
        SearchResultsDto resultsDto = new( searchReply.Value.TotalMatches, searchReply.Value.Results, estimatesReply );
        return Results.Ok( resultsDto );
    }
    static async Task<IResult> GetEstimates( HttpContext http, InventoryRepository inventory )
    {
        IQueryCollection query = http.Request.Query;
        var productIds = Utils.ParseGuidList( query["ProductIds"] );
        var posX = Utils.ParseInt( query["PosX"] );
        var posY = Utils.ParseInt( query["PosY"] );

        if (productIds is null)
            return Results.BadRequest( "Invalid Product Ids." );
        if (posX is null || posY is null)
            return Results.BadRequest( "Invalid Address." );

        var estimates = await inventory.GetDeliveryEstimates( productIds, new AddressDto( posX.Value, posX.Value ) );
        return Results.Ok( estimates );
    }
    static async Task<IResult> GetDetails( HttpContext http, ProductDetailsRepository repository, InventoryRepository inventory )
    {
        IQueryCollection query = http.Request.Query;
        var productId = Utils.ParseGuid( query["ProductId"] );
        var posX = Utils.ParseInt( query["PosX"] );
        var posY = Utils.ParseInt( query["PosY"] );
        
        if (productId is null)
            return Results.BadRequest( "Invalid Product Id." );

        ProductDto? result = await repository.GetDetails( productId.Value );
        
        if (result is null || posX is null || posY is null)
            return result is not null
                ? Results.Ok( result )
                : Results.NotFound();

        var shippingDays = await inventory.GetDeliveryEstimates( [result.Value.Id], new AddressDto( posX.Value, posY.Value ) );
        return Results.Ok( result.Value with { ShippingDays = shippingDays.FirstOrDefault() } );
    }
}