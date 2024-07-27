namespace CatalogApplication.Middleware;

internal static class MiddlewareConfiguration
{
    internal static void ConfigureMiddleware( this WebApplication app )
    {
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        app.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}