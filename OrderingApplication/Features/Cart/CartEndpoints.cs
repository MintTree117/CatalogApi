using Microsoft.AspNetCore.Mvc;
using OrderingApplication.Extentions;
using OrderingInfrastructure.Features.Cart;

namespace OrderingApplication.Features.Cart;

internal static class CartEndpoints
{
    internal static void MapCartEndpoints( this IEndpointRouteBuilder app )
    {
        app.MapPost( "api/cart/get-updated",
               async ( [FromBody] List<CartItemDto> items, HttpContext http, ICartRepository repository ) =>
               (await repository.GetUpdatedCart( http.UserId(), items.Models() ))
               .Dtos().GetIResult() )
           .RequireAuthorization();

        app.MapPost( "api/cart/add-or-update",
               async ( [FromBody] CartItemDto item, HttpContext http, ICartRepository repository ) =>
               (await repository.AddOrUpdate( item.ProductId, http.UserId(), item.Quantity ))
               .GetIResult() )
           .RequireAuthorization();

        app.MapPost( "api/cart/delete",
               async ( [FromBody] CartItemDto item, HttpContext http, ICartRepository repository ) =>
               (await repository.Delete( item.ProductId, http.UserId() )).GetIResult() )
           .RequireAuthorization();

        app.MapPost( "api/cart/clear",
               async ( HttpContext http, ICartRepository repository ) =>
                   (await repository.Empty( http.UserId() ))
                   .GetIResult() )
           .RequireAuthorization();
    }
}