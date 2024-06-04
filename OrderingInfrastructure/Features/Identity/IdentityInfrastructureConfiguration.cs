using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderingInfrastructure.Features.Identity.Repositories;

namespace OrderingInfrastructure.Features.Identity;

internal static class IdentityInfrastructureConfiguration
{
    internal static void ConfigureIdentityInfrastructure( this WebApplicationBuilder builder )
    {
        builder.Services.AddDbContext<IdentityDbContext>( GetDatabaseOptions );
        builder.Services.AddScoped<IIdentityAddressRepository, IdentityAddressRepository>();
    }

    static void GetDatabaseOptions( DbContextOptionsBuilder options )
    {
        options.UseInMemoryDatabase( "IdentityDb" );
    }
}