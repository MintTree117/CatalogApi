namespace CatalogApplication.Seeding;

internal static class SeedingConfiguration
{
    internal static void AddSeeding( this WebApplicationBuilder builder )
    {
        builder.Services.AddSingleton<SeedingService>();
    }
    
    internal static async Task Seed( this WebApplication app )
    {
        var seeder = app.Services.GetRequiredService<SeedingService>();
        await seeder.SeedDatabase();
    }
}