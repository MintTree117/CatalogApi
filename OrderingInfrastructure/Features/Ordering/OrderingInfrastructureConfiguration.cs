using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderingInfrastructure.Features.Ordering.Repositories;

namespace OrderingInfrastructure.Features.Ordering;

internal static class OrderingInfrastructureConfiguration
{
    internal static void ConfigureOrderingInfrastructure( this WebApplicationBuilder builder )
    {
        builder.Services.AddDbContext<OrderingDbContext>( GetDatabaseOptions );
        builder.Services.AddScoped<IOrderingRepository, OrderingRepository>();
        builder.Services.AddScoped<IOrderingUtilityRepository, OrderingUtilityRepository>();
    }

    static void GetDatabaseOptions( DbContextOptionsBuilder options )
    {
        options.UseInMemoryDatabase( "OrderingDb" );
    }
}