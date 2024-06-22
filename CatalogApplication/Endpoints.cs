using CatalogApplication.Repositories;
using CatalogApplication.Types._Common.Geography;
using CatalogApplication.Types.Brands.Dtos;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Products.Dtos;
using CatalogApplication.Types.Search.Dtos;
using CatalogApplication.Types.Search.Local;
using Microsoft.AspNetCore.Mvc;

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
        app.MapGet( "api/details", static async ( [FromQuery] string productId, [FromQuery] string? posX, [FromQuery] string? posY, ProductDetailsRepository details, InventoryRepository inventory ) =>
            await GetDetails( productId, posX, posY, details, inventory ) );
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
            ParseGuid( query["CategoryId"] ),
            ParseGuidList( query["BrandIds"] ),
            ParseInt( query["MinPrice"] ),
            ParseInt( query["MaxPrice"] ),
            ParseBool( query["IsInStock"] ),
            ParseBool( query["IsFeatured"] ),
            ParseBool( query["IsOnSale"] ) ?? false,
            ParseInt( query["Page"] ) ?? 0,
            ParseInt( query["PageSize"] ) ?? 5,
            ParseInt( query["SortBy"] ) ?? 0,
            ParseInt( query["PosX"] ),
            ParseInt( query["PosY"] )
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
    static async Task<IResult> GetDetails( string productId, string? posX, string? posY, ProductDetailsRepository repository, InventoryRepository inventory )
    {
        if (!Guid.TryParse( productId, out Guid id ))
            return Results.BadRequest( "Invalid Product Id." );

        ProductDto? result = await repository.GetDetails( id );
        
        if (result is null || !int.TryParse( posX, out int x ) || !int.TryParse( posY, out int y ))
            return result is not null
                ? Results.Ok( result )
                : Results.NotFound();

        var shippingDays = await inventory.GetDeliveryEstimates( [result.Value.Id], new AddressDto( x, y ) );
        return Results.Ok( result.Value with { ShippingDays = shippingDays.FirstOrDefault() } );
    }

    static List<Guid>? ParseGuidList( string? value )
    {
        if (string.IsNullOrEmpty( value ))
            return null;

        IEnumerable<Guid> guids = value.Split( ',' ).Select( static x => Guid.TryParse( x, out Guid guid ) ? guid : Guid.Empty );
        return guids.ToList();
    }
    static Guid? ParseGuid( string? value )
    {
        if (Guid.TryParse( value, out Guid result ))
            return result;
        return null;
    }
    static int? ParseInt( string? value )
    {
        if (int.TryParse( value, out int result ))
            return result;
        return null;
    }
    static bool? ParseBool( string? value )
    {
        if (bool.TryParse( value, out bool result ))
            return result;
        return null;
    }
}