using System.Data;
using CartDomain.Cart;
using Dapper;
using OrderingDomain.Optionals;

namespace OrderingInfrastructure.Features.Cart;

internal sealed class CartRepository( CartDbContext cartDb ) : ICartRepository
{
    readonly CartDbContext db = cartDb;

    public async Task<Replies<CartItem>> GetUpdatedCart( string userId, List<CartItem> itemsFromClient )
    {
        const string sql =
            $"""
             MERGE INTO Cart AS target
             USING (
                 SELECT {CartDbConsts.UserId}, {CartDbConsts.ProductId}, {CartDbConsts.Quantity}, GETDATE() AS {CartDbConsts.Timestamp}
                 FROM {CartDbConsts.CartItems}
             ) AS source
             ON target.{CartDbConsts.UserId} = source.{CartDbConsts.UserId} AND target.{CartDbConsts.ProductId} = source.{CartDbConsts.ProductId}
             WHEN MATCHED THEN
                 UPDATE SET {CartDbConsts.Quantity} = target.{CartDbConsts.Quantity} + source.{CartDbConsts.Quantity}, {CartDbConsts.Timestamp} = GETDATE()
             WHEN NOT MATCHED THEN
                 INSERT ({CartDbConsts.UserId}, {CartDbConsts.ProductId}, {CartDbConsts.Quantity}, {CartDbConsts.Timestamp})
                 VALUES (source.{CartDbConsts.UserId}, source.{CartDbConsts.ProductId}, source.{CartDbConsts.Quantity}, source.{CartDbConsts.Timestamp});
             """;

        DataTable table = GetCartItemsTable( userId, itemsFromClient );
        DynamicParameters parameters = new();
        parameters.Add( CartDbConsts.UserId, userId );
        parameters.Add( CartDbConsts.CartItems, table.AsTableValuedParameter( CartDbConsts.CartItemsTvp ) );
        return await db.QueryAsync<CartItem>( sql, parameters );
    }
    public async Task<Reply<bool>> AddOrUpdate( Guid productId, string userId, int quantity )
    {
        const string sql =
            $"""
             MERGE INTO {CartDbConsts.CartItems} AS target
             USING (
                 VALUES (@{CartDbConsts.UserId}, @{CartDbConsts.ProductId}, @{CartDbConsts.Quantity}, GETDATE()) -- Parameters for UserID, ProductID, and Quantity
             ) AS source ({CartDbConsts.UserId}, {CartDbConsts.ProductId}, {CartDbConsts.Quantity}, {CartDbConsts.Timestamp})
             ON target.{CartDbConsts.UserId} = source.{CartDbConsts.UserId} AND target.{CartDbConsts.ProductId} = source.{CartDbConsts.ProductId}
             WHEN MATCHED THEN
                 UPDATE SET {CartDbConsts.Quantity} = target.{CartDbConsts.Quantity} + source.{CartDbConsts.Quantity}, {CartDbConsts.Timestamp} = GETDATE() -- Update existing record
             WHEN NOT MATCHED THEN
                 INSERT ({CartDbConsts.UserId}, {CartDbConsts.ProductId}, {CartDbConsts.Quantity}, Timestamp)
                 VALUES (source.{CartDbConsts.UserId}, source.{CartDbConsts.ProductId}, source.{CartDbConsts.Quantity}, source.{CartDbConsts.Timestamp}); -- Insert new record
             """;

        DynamicParameters parameters = new();
        parameters.Add( CartDbConsts.ProductId, productId );
        parameters.Add( CartDbConsts.UserId, userId );
        parameters.Add( CartDbConsts.Quantity, quantity );
        return await db.ExecuteAsync( sql, parameters );
    }
    public async Task<Reply<bool>> Delete( Guid productId, string userId )
    {
        const string sql =
            """
            DELETE FROM Cart_Items
            WHERE UserId = @UserId
            AND ProductId = @ProductId;
            """;

        DynamicParameters parameters = new();
        parameters.Add( CartDbConsts.ProductId, productId );
        parameters.Add( CartDbConsts.UserId, userId );
        return await db.ExecuteAsync( sql, parameters );
    }
    public async Task<Reply<bool>> Empty( string userId )
    {
        const string sql =
            """
            DELETE FROM Cart_Items
            WHERE UserId = @UserId;
            """;

        DynamicParameters parameters = new();
        parameters.Add( CartDbConsts.UserId, userId );
        return await db.ExecuteAsync( sql, parameters );
    }

    static DataTable GetCartItemsTable( string userId, List<CartItem> items )
    {
        DataTable table = new();
        table.Columns.Add( CartDbConsts.ProductId, typeof( int ) );
        table.Columns.Add( CartDbConsts.UserId, typeof( int ) );
        table.Columns.Add( CartDbConsts.Quantity, typeof( int ) );

        foreach ( CartItem d in items ) {
            DataRow row = table.NewRow();
            row[CartDbConsts.ProductId] = d.ProductId;
            row[CartDbConsts.UserId] = userId;
            row[CartDbConsts.Quantity] = d.Quantity;
            table.Rows.Add( row );
        }

        return table;
    }
}