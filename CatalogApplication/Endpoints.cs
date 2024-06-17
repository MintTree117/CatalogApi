using CatalogApplication.Repositories;
using CatalogApplication.Types;
using CatalogApplication.Types._Common.Geography;
using CatalogApplication.Types.Brands.Dtos;
using CatalogApplication.Types.Categories;
using CatalogApplication.Types.Products.Dtos;
using CatalogApplication.Types.Search.Dtos;
using CatalogApplication.Types.Search.Local;
using Dapper;
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
        app.MapGet( "api/search-query", static async ( HttpContext http, ProductSearchRepository products, InventoryRepository inventory ) =>
            await GetSearchQuery( http, products ) );
        app.MapGet( "api/search", static async ( HttpContext http, ProductSearchRepository products, InventoryRepository inventory ) =>
            await GetSearch( http, products, inventory ) );
        app.MapGet( "api/details", static async ( [FromQuery] Guid productId, ProductDetailsRepository repository ) =>
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
    static async Task<IResult> GetSearchQuery( HttpContext http, ProductSearchRepository products)
    {
        await Task.Delay( 1000 );
        
        IQueryCollection query = http.Request.Query;

        // Parse Pagination
        int? page = ParseInt( query["Page"] );
        int? rows = ParseInt( query["PageSize"] );
        string orderBy = query["OrderBy"].ToString();

        Pagination pagination = new( page ?? 0, rows ?? 10, orderBy );

        // Parse SearchFiltersDto
        SearchFilters productSearchFilters = new(
            ParseGuidList( query["BrandIds"] ),
            ParseInt( query["MinPrice"] ),
            ParseInt( query["MaxPrice"] ),
            ParseBool( query["IsInStock"] ),
            ParseBool( query["IsFeatured"] ),
            ParseBool( query["IsOnSale"] ) ?? false
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

        ProductSearchRepository.BuildSqlQuery( queryRequest, out string sql, out DynamicParameters parameters );
        Console.WriteLine( "########################" );
        Console.WriteLine( sql );

        return Results.Ok( sql );
    }
    static async Task<IResult> GetSearch( HttpContext http, ProductSearchRepository products, InventoryRepository inventory )
    {
        IQueryCollection query = http.Request.Query;

        // CATEGORIES
        List<Guid>? categoryIds = ParseGuidList( query["CategoryIds"] );

        // SEARCH TEXT
        string? searchText = query["SearchText"];
        
        // FILTERS
        SearchFilters productSearchFilters = new(
            ParseGuidList( query["BrandIds"] ),
            ParseInt( query["MinPrice"] ),
            ParseInt( query["MaxPrice"] ),
            ParseBool( query["IsInStock"] ),
            ParseBool( query["IsFeatured"] ),
            ParseBool( query["IsOnSale"] ) ?? false
        );

        // ADDRESS
        int? x = ParseInt( query["PosX"] );
        int? y = ParseInt( query["PosY"] );
        AddressDto? deliveryAddress = x is null || y is null ? null : new AddressDto( x.Value, y.Value );

        // PAGINATION
        int? page = ParseInt( query["Page"] );
        int? rows = ParseInt( query["PageSize"] );
        string orderBy = query["SortBy"].ToString();
        Pagination pagination = new( page ?? 0, rows ?? 10, orderBy );
        
        // LOCAL MODEL
        SearchQueryRequest queryRequest = new(
            searchText,
            categoryIds?.Count > 0 ? categoryIds : null,
            productSearchFilters,
            pagination
        );
        
        // SEARCH
        SearchQueryReply? searchReply = await products.GetSearch( queryRequest );
        if (searchReply is null)
            return Results.NotFound();
        
        // SHIPPING
        List<int> estimatesReply = await inventory.GetDeliveryEstimates( searchReply.Value.Results, deliveryAddress );
        
        // FINISH
        SearchResultDto resultDto = new( searchReply.Value.TotalMatches, searchReply.Value.Results, estimatesReply );
        return Results.Ok( resultDto );
    }
    static async Task<IResult> GetDetails( Guid productId, ProductDetailsRepository repository )
    {
        ProductDto? result = await repository.GetDetails( productId );
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