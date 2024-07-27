namespace CatalogApplication.Database;

internal static class DatabaseConfiguration
{
    internal static void AddDatabase( this WebApplicationBuilder builder )
    {
        builder.Services.AddSingleton<IDapperContext, DapperContext>();
    }
}