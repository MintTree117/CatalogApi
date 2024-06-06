using CatalogApplication.Repositories;
using CatalogApplication.Types;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Filters.Dtos;
using CatalogApplication.Types.Products.Dtos;
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
        app.MapGet( "api/search", async ( HttpContext http, ProductSearchRepository repository ) =>
            await GetSearch( http, repository ) );
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
    static async Task<IResult> GetSearch( HttpContext http, ProductSearchRepository repository )
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
            ParseGuidList( query["ProductSearchFilters.PriceRangeIds"] ),
            ParseGuidList( query["ProductSearchFilters.RatingLevelIds"] ),
            ParseBool( query["ProductSearchFilters.IsInStock"] ),
            ParseBool( query["ProductSearchFilters.IsFeatured"] ),
            ParseBool( query["ProductSearchFilters.IsOnSale"] )
        );

        // Parse CategoryIds
        List<Guid>? categoryIds = ParseGuidList( query["CategoryIds"] );

        SearchRequest request = new(
            categoryIds?.Count > 0 ? categoryIds : null,
            productSearchFilters,
            pagination
        );
        
        SearchReply? result = await repository.GetSearch( request );
        return result is not null
            ? Results.Ok( result )
            : Results.NotFound();
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