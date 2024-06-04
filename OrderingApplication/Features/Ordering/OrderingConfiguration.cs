using OrderingApplication.Features.Ordering.Services;
using OrderingApplication.Features.Ordering.Systems;
using OrderingInfrastructure;

namespace OrderingApplication.Features.Ordering;

internal static class OrderingConfiguration
{
    internal static void ConfigureOrdering( this WebApplicationBuilder builder )
    {
        builder.ConfigureInfrastructure();
        builder.Services.AddSingleton<OrderLocationCache>();
        builder.Services.AddScoped<OrderLocationService>();
        builder.Services.AddScoped<OrderPlacingSystem>();
        builder.Services.AddScoped<OrderUpdatingSystem>();
        builder.Services.AddScoped<OrderCancellingSystem>();
        builder.Services.AddHostedService<OrderStateFlaggingService>();
        builder.Services.AddHostedService<OrderPendingCancelService>();
    }
}