using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OrderingInfrastructure.Features.Cart;

internal static class CartInfrastructureConfiguration
{
    internal static void ConfigureCartInfrastructure( this WebApplicationBuilder builder )
    {
        builder.Services.AddSingleton<CartDbContext>();
        builder.Services.AddSingleton<ICartRepository, CartRepository>();
    }
}