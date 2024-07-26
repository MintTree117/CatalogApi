namespace CatalogApplication.Utilities;

internal static class CorsConfiguration
{
    internal static void ConfigureCors( this WebApplicationBuilder builder )
    {
        var origins = builder.Configuration.GetSection( "AllowedOrigins" ).Get<string[]>() 
            ?? throw new Exception( "Failed to get AllowedOrigins from configuration during startup." );
        
        builder.Services.AddCors( options => {
            options.AddDefaultPolicy( cors => cors
                .WithOrigins( origins )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials() );
        } );
    }
}