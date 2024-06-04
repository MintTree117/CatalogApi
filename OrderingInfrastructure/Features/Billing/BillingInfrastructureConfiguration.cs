using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OrderingInfrastructure.Features.Billing;

public static class BillingInfrastructureConfiguration
{
    internal static void ConfigureBillingInfrastructure( this WebApplicationBuilder builder )
    {
        builder.Services.AddDbContext<BillingDbContext>( GetDatabaseOptions );
        builder.Services.AddScoped<IBillingRepository, BillingRepository>();
    }

    static void GetDatabaseOptions( DbContextOptionsBuilder options )
    {
        options.UseInMemoryDatabase( "BillingDb" );
    }
}