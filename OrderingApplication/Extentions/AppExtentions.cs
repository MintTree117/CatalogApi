using OrderingApplication.Features.Billing;
using OrderingApplication.Features.Cart;
using OrderingApplication.Features.Identity;
using OrderingApplication.Features.Ordering;

namespace OrderingApplication.Extentions;

internal static class AppExtentions
{
    internal static void HandleSwagger( this WebApplication app )
    {
        if (!app.Environment.IsDevelopment()) 
            return;
        
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    internal static void MapEndpoints( this WebApplication app )
    {
        app.MapIdentityEndpoints();
        app.MapBillingEndpoints();
        app.MapCartEndpoints();
        app.MapOrderingEndpoints();
    }
    internal static void AddCustomMiddleware( this WebApplication app )
    {
        app.AddIdentityMiddleware();
    }
}