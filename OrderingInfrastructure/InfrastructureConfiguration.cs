using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OrderingInfrastructure.Email;
using OrderingInfrastructure.Http;
using OrderingInfrastructure.Features.Billing;
using OrderingInfrastructure.Features.Cart;
using OrderingInfrastructure.Features.Identity;
using OrderingInfrastructure.Features.Ordering;

namespace OrderingInfrastructure;

public static class InfrastructureConfiguration
{
    public static void ConfigureInfrastructure( this WebApplicationBuilder builder )
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSingleton<IEmailSender, EmailSender>();
        builder.Services.AddSingleton<IHttpService, HttpService>();
        builder.ConfigureIdentityInfrastructure();
        builder.ConfigureOrderingInfrastructure();
        builder.ConfigureCartInfrastructure();
        builder.ConfigureBillingInfrastructure();
    }
}