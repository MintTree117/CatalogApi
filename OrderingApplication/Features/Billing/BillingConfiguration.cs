namespace OrderingApplication.Features.Billing;

internal static class BillingConfiguration
{
    internal static void ConfigureBilling( this WebApplicationBuilder builder )
    {
        builder.Services.AddScoped<BillingService>();
    }
}