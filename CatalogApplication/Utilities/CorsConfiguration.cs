namespace CatalogApplication.Utilities;

internal static class CorsConfiguration
{
    internal static void ConfigureCors( this WebApplicationBuilder builder )
    {
        var origins = builder.Configuration.GetSection( "AllowedOrigins" ).Get<string[]>() 
            ?? throw new Exception( "Failed to get AllowedOrigins from configuration during startup." );

        string[] testOrigins = ["https://martinsorderingapi.azurewebsites.net", "https://martinscatalogapi.azurewebsites.net", "https://happy-bush-0b0f3e80f.5.azurestaticapps.net"];
        
        builder.Services.AddCors( options => {
            options.AddDefaultPolicy( cors => cors
                .WithOrigins( testOrigins )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials() );
        } );
    }
}