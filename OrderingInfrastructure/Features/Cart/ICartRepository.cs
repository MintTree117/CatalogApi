using CartDomain.Cart;
using OrderingDomain.Optionals;

namespace OrderingInfrastructure.Features.Cart;

public interface ICartRepository
{
    Task<Replies<CartItem>> GetUpdatedCart( string userId, List<CartItem> itemsFromClient );
    Task<Reply<bool>> AddOrUpdate( Guid productId, string userId, int quantity );
    Task<Reply<bool>> Delete( Guid productId, string userId );
    Task<Reply<bool>> Empty( string userId );
}