using System.Net;

namespace CatalogApplication.Middleware;

internal sealed class GlobalExceptionHandlerMiddleware( RequestDelegate next )
{
    readonly RequestDelegate _next = next;

    public async Task InvokeAsync( HttpContext context )
    {
        try
        {
            await _next( context );
        }
        catch ( Exception ex )
        {
            await HandleExceptionAsync( context, ex );
        }
    }

    static Task HandleExceptionAsync( HttpContext context, Exception exception )
    {
        EndpointLogger.LogException( exception, "An unhandled exception has occurred, caught by custom middleware." );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

        var result = new {
            message = "An unexpected error occurred. Please try again later."
        };

        return context.Response.WriteAsJsonAsync( result );
    }
}