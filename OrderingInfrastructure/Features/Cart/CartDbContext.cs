using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OrderingDomain.Optionals;
using OrderingDomain.ValueTypes;

namespace OrderingInfrastructure.Features.Cart;

public sealed class CartDbContext
{
    // Fields
    readonly string _connectionString = string.Empty;
    readonly bool _noString = true;

    // Public Methods
    public CartDbContext( IConfiguration config )
    {
        try {
            _connectionString = config.GetConnectionString( "DefaultConnection" ) ?? string.Empty;

            if (!string.IsNullOrEmpty( _connectionString ))
                _noString = false;
        }
        catch ( Exception e ) {
            Console.WriteLine( $"Failed to get connection string in DapperContext! : {e}" );
        }
    }
    public async Task<Replies<T>> QueryAsync<T>( string sql, DynamicParameters? parameters = null )
    {
        await using SqlConnection c = await GetOpenConnection();

        if (c.State != ConnectionState.Open)
            return Replies<T>.Error( "Connection state not open." );

        try {
            IEnumerable<T> enumerable = await c.QueryAsync<T>( sql, parameters, commandType: CommandType.Text );
            return Replies<T>.With( enumerable );
        }
        catch ( Exception e ) {
            Console.WriteLine( e );
            return default;
        }
    }
    public async Task<Reply<T>> QueryFirstOrDefaultAsync<T>( string sql, DynamicParameters? parameters = null )
    {
        SqlConnection c = await GetOpenConnection();

        if (c.State != ConnectionState.Open)
            return Reply<T>.None( "Problem.Network" );

        try {
            var item = await c.QueryFirstOrDefaultAsync<T>( sql, parameters, commandType: CommandType.Text );
            return Reply<T>.Maybe( item );
        }
        catch ( Exception e ) {
            Console.WriteLine( e );
            return Reply<T>.Exception( e, "Message" );
        }
    }
    public async Task<SqlConnection> GetOpenConnection()
    {
        if (_noString)
            return new SqlConnection( string.Empty );

        SqlConnection connection = new( _connectionString );
        await connection.OpenAsync();
        return connection;
    }

    public async Task<Reply<bool>> ExecuteAsync( string sql, DynamicParameters? parameters = null )
    {
        await using SqlConnection c = await GetOpenConnection();

        if (c.State != ConnectionState.Open)
            return Reply<bool>.None( "Connection state not open." );

        try {
            int result = await c.ExecuteAsync( sql, parameters, commandType: CommandType.Text );
            return result > 0
                ? Reply<bool>.With( true )
                : Reply<bool>.None( "Failed to execute async dapper call." );
        }
        catch ( Exception e ) {
            Console.WriteLine( e );
            return Reply<bool>.Exception( e, "Failed to execute async dapper call." );
        }
    }
}