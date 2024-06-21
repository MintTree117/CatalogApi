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
        app.MapGet( "api/details", static async ( [FromQuery] string productId, ProductDetailsRepository repository ) =>
            await GetDetails( productId, repository ) );
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
        List<int> estimatesReply = await inventory.GetDeliveryEstimates( searchReply.Value.Results, deliveryAddress );
        
        // FINISH
        SearchResultsDto resultsDto = new( searchReply.Value.TotalMatches, searchReply.Value.Results, estimatesReply );
        return Results.Ok( resultsDto );
    }
    static async Task<IResult> GetDetails( string productId, ProductDetailsRepository repository )
    {
        if (!Guid.TryParse( productId, out Guid id ))
            return Results.BadRequest( "Invalid Product Id." );
        ProductDto? result = await repository.GetDetails( id );
        return result is not null
            ? Results.Ok( result )
            : Results.NotFound();
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