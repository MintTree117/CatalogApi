using CartDomain.Cart;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Cart;

internal static class CartExtentions
{
    internal static Replies<CartItemDto> Dtos( this Replies<CartItem> models )
    {
        List<CartItemDto> dtos = [];
        dtos.AddRange( from m in models.Enumerable select MapToDto( m ) );
        return Replies<CartItemDto>.With( dtos );
    }
    internal static List<CartItem> Models( this IEnumerable<CartItemDto> dtos )
    {
        List<CartItem> models = [];
        models.AddRange( from d in dtos select MapToModel( d ) );
        return models;
    }

    static CartItemDto MapToDto( CartItem item )
        => CartItemDto.New( item.ProductId, item.Quantity );
    static CartItem MapToModel( CartItemDto dto )
        => CartItem.WithoutUserId( dto.ProductId, dto.Quantity );
}