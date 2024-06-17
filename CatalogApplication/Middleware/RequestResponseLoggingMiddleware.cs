using System.Text;

namespace CatalogApplication.Middleware;

internal sealed class RequestResponseLoggingMiddleware( RequestDelegate next )
{
    readonly RequestDelegate _next = next;

    public async Task InvokeAsync( HttpContext context )
    {
        try
        {
            // Log the request
            var requestLog = FormatRequest( context.Request );
            EndpointLogger.LogInformation( requestLog );

            // Copy a pointer to the original response body stream
            var originalBodyStream = context.Response.Body;

            // Create a new memory stream to capture the response
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Continue down the Middleware pipeline, eventually returning to this class
            await _next( context );

            // Log the response
            var responseLog = await FormatResponse( context, responseBody );
            EndpointLogger.LogInformation( responseLog );

            // Copy the contents of the new memory stream (which contains the response) to the original stream
            responseBody.Seek( 0, SeekOrigin.Begin );
            await responseBody.CopyToAsync( originalBodyStream );
        }
        catch ( Exception ex )
        {
            EndpointLogger.LogError( $"Middleware Exception: {ex.Message}" );
            throw;
        }
    }

    static string FormatRequest( HttpRequest request )
    {
        request.EnableBuffering();

        if (request.ContentLength > 0)
        {
            var buffer = new byte[Convert.ToInt32( request.ContentLength )];
            Encoding.UTF8.GetString( buffer );
            request.Body.Position = 0;
        }

        var message = $"Endpoint Hit: {request.Path}";
        return message;
    }
    static async Task<string> FormatResponse( HttpContext context, MemoryStream responseBody )
    {
        context.Response.Body.Seek( 0, SeekOrigin.Begin );
        var responseText = await new StreamReader( responseBody ).ReadToEndAsync();
        var endpointPath = context.Request.Path;
        return $"Endpoint Response: {endpointPath} : Status Code = {context.Response.StatusCode} : Body = {responseText}";
    }
}