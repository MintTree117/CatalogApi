namespace CartDomain.Cart;

public sealed class CartItem
{
    public CartItem() { }

    public CartItem( Guid userId, Guid productId, int quantity )
    {
        UserId = userId;
        ProductId = productId;
        Quantity = quantity;
    }

    public static CartItem WithoutUserId( Guid productId, int quantity ) => 
        new( Guid.Empty, productId, quantity );

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}