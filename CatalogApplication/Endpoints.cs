using CatalogApplication.Repositories;
using CatalogApplication.Types._Common.Geography;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Filters.Dtos;
using CatalogApplication.Types.Products.Dtos;
using CatalogApplication.Types.Products.Models;
using Microsoft.AspNetCore.Mvc;

namespace CatalogApplication;

internal static class Endpoints
{
    internal static void MapEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapGet( "api/categories", async ( CategoryRepository repository ) => 
            await GetCategories( repository ) );
        app.MapGet( "api/filters", async ( FilterRepository repository ) =>
            await GetFilters( repository ) );
        app.MapGet( "api/search", async ( HttpContext http, ProductSearchRepository products, InventoryRepository inventory ) =>
            await GetSearch( http, products, inventory ) );
        app.MapGet( "api/details", async ( [FromQuery] Guid productId, ProductDetailsRepository repository ) =>
            await GetDetails( productId, repository ) );
    }

    static async Task<IResult> GetCategories( CategoryRepository repository )
    {
        List<Category> result = (await repository.GetCategories()).ToList();
        return result.Count > 0
            ? Results.Ok( result )
            : Results.NotFound();
    }
    static async Task<IResult> GetFilters( FilterRepository repository )
    {
        FiltersReply? result = await repository.GetFilters();
        return result is not null
            ? Results.Ok( result )
            : Results.NotFound();
    }
    static async Task<IResult> GetSearch( HttpContext http, ProductSearchRepository products, InventoryRepository inventory )
    {
        IQueryCollection query = http.Request.Query;

        // Parse Pagination
        int? page = ParseInt( query["Pagination.Page"] );
        int? rows = ParseInt( query["Pagination.Rows"] );
        string orderBy = query["Pagination.OrderBy"].ToString();

        Pagination pagination = new( page ?? 0, rows ?? 10, orderBy );

        // Parse SearchFiltersDto
        SearchFiltersDto productSearchFilters = new(
            ParseGuidList( query["ProductSearchFilters.BrandIds"] ),
            ParseInt( query["ProductSearchFilters.MinimumPrice"] ),
            ParseInt( query["ProductSearchFilters.MaximumPrice"] ),
            ParseInt( query["ProductSearchFilters.MinimumRating"] ),
            ParseBool( query["ProductSearchFilters.IsInStock"] ),
            ParseBool( query["ProductSearchFilters.IsFeatured"] ),
            ParseBool( query["ProductSearchFilters.IsOnSale"] ) ?? false
        );

        // Parse CategoryIds
        List<Guid>? categoryIds = ParseGuidList( query["CategoryIds"] );
        
        // Parse SearchText
        string? searchText = query["SearchText"];

        // Parse Address
        int? x = ParseInt( query["Address.X"] );
        int? y = ParseInt( query["Address.Y"] );
        AddressDto? deliveryAddress = x is null || y is null ? null : new AddressDto( x.Value, y.Value );

        SearchQueryRequest queryRequest = new(
            searchText,
            categoryIds?.Count > 0 ? categoryIds : null,
            productSearchFilters,
            pagination
        );
        
        SearchQueryReply? searchReply = await products.GetSearch( queryRequest );
        if (searchReply is null)
            return Results.NotFound();

        List<int> estimatesReply = await inventory.GetDeliveryEstimates( searchReply.Value.Results, deliveryAddress );
        SearchReply reply = new( searchReply.Value.TotalMatches, searchReply.Value.Results, estimatesReply );
        return Results.Ok( reply );
    }
    static async Task<IResult> GetDetails( Guid productId, ProductDetailsRepository repository )
    {
        DetailsDto? result = await repository.GetDetails( productId );
        return result is not null
            ? Results.Ok( result )
            : Results.NotFound();
    }

    static List<Guid>? ParseGuidList( string? value )
    {
        if (string.IsNullOrEmpty( value ))
            return null;

        IEnumerable<Guid> guids = value.Split( ',' ).Select( x => Guid.TryParse( x, out Guid guid ) ? guid : Guid.Empty );
        return guids.ToList();
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