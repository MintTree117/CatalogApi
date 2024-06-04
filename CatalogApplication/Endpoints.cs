using CatalogApplication.Database;
using Microsoft.AspNetCore.Mvc;

namespace CatalogApplication;

internal static class Endpoints
{
    internal static void MapEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapGet( "api/categories", async ( IDapperContext dapper ) => 
            await GetCategories( dapper ) );
        app.MapGet( "api/filters", async ( IDapperContext dapper ) =>
            await GetSpecLookups( dapper ) );
        app.MapGet( "api/search", async ( IDapperContext dapper ) =>
            await GetProductSearch( dapper ) );
        app.MapGet( "api/details", async ( [FromQuery] Guid productId, IDapperContext dapper ) =>
            await GetProductDetails( productId, dapper ) );
    }

    static async Task<IResult> GetCategories( IDapperContext dapper )
    {
        await Task.Delay( 1000 );
        return Results.Empty;
    }
    static async Task<IResult> GetSpecLookups( IDapperContext dapper )
    {
        await Task.Delay( 1000 );
        return Results.Empty;
    }
    static async Task<IResult> GetProductSearch( IDapperContext dapper )
    {
        await Task.Delay( 1000 );
        return Results.Empty;
    }
    static async Task<IResult> GetProductDetails( Guid productId, IDapperContext dapper )
    {
        await Task.Delay( 1000 );
        return Results.Empty;
    }
}