using CatalogApplication.Repositories.Features;

namespace CatalogApplication.Repositories;

internal static class RepositoryConfiguration
{
    internal static void AddRepositories( this WebApplicationBuilder builder )
    {
        builder.Services.AddSingleton<BrandRepository>();
        builder.Services.AddSingleton<CategoryRepository>();
        builder.Services.AddSingleton<InventoryRepository>();
        builder.Services.AddSingleton<ProductDetailsRepository>();
        builder.Services.AddSingleton<ProductSearchRepository>();
        builder.Services.AddSingleton<ProductSpecialsRepository>();
    }
}