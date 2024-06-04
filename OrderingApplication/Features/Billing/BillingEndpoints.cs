using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingInfrastructure.Features.Billing;

namespace OrderingApplication.Features.Billing;

internal static class BillingEndpoints
{
    internal static void MapBillingEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapGet( "api/billing/view/invoice",
               async ( [FromQuery] Guid orderId, IBillingRepository repository ) =>
               await GetInvoice( orderId, repository ) )
           .RequireAuthorization();

        app.MapGet( "api/billing/view/bill",
               async ( [FromQuery] Guid orderId, IBillingRepository repository ) =>
               await GetInvoice( orderId, repository ) )
           .RequireAuthorization();
    }

    static async Task<IResult> GetInvoice( Guid orderId, IBillingRepository repository ) => (
        await repository.GetInvoice( orderId )).GetIResult();

    static async Task<IResult> GetBill( Guid orderId, IBillingRepository repository ) => (
        await repository.GetBill( orderId )).GetIResult();
}