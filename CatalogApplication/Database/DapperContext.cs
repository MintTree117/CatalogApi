using System.Data;
using CatalogApplication.Types._Common.ReplyTypes;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Database;

internal sealed class DapperContext : IDapperContext
{
    // Fields
    const string InvalidConnectionMessage = "Invalid Connection State: ";
    const string ExceptionMessage = "An internal server error occured.";
    readonly string _connectionString = string.Empty;
    readonly bool _noString = true;

    // Public Methods
    internal DapperContext( IConfiguration config )
    {
        try {
            _connectionString = config["ConnectionString"] ?? string.Empty;

            if (!string.IsNullOrEmpty( _connectionString ))
                _noString = false;
        }
        catch ( Exception e ) {
            Console.WriteLine( $"Failed to get connection string in DapperContext! : {e}" );
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
    public async Task<Replies<T>> QueryAsync<T>( string sql, DynamicParameters? parameters = null )
    {
        await using SqlConnection c = await GetOpenConnection();

        if (c.State != ConnectionState.Open)
            return Replies<T>.Error( InvalidConnectionMessage + c.State );

        try {
            IEnumerable<T> enumerable = await c.QueryAsync<T>( sql, parameters, commandType: CommandType.Text );
            return Replies<T>.With( enumerable );
        }
        catch ( Exception e ) {
            Console.WriteLine( e );
            return Replies<T>.Error( ExceptionMessage );
        }
    }
    public async Task<Reply<T>> QueryFirstOrDefaultAsync<T>( string sql, DynamicParameters? parameters = null )
    {
        await using SqlConnection c = await GetOpenConnection();

        if (c.State != ConnectionState.Open)
            return Reply<T>.None( InvalidConnectionMessage + c.State );

        try {
            var item = await c.QueryFirstOrDefaultAsync<T>( sql, parameters, commandType: CommandType.Text );
            return item is not null
                ? Reply<T>.With( item )
                : Reply<T>.None( "Not Found." );
        }
        catch ( Exception e ) {
            Console.WriteLine( e );
            return Reply<T>.None( ExceptionMessage );
        }
    }
    public async Task<Reply<int>> ExecuteAsync( string sql, DynamicParameters? parameters = null )
    {
        await using SqlConnection c = await GetOpenConnection();

        if (c.State != ConnectionState.Open)
            return Reply<int>.None( InvalidConnectionMessage + c.State );

        try {
            int result = await c.ExecuteAsync( sql, parameters, commandType: CommandType.Text );
            return result > 0
                ? Reply<int>.With( result )
                : Reply<int>.None( "No Rows Altered" );
        }
        catch ( Exception e ) {
            Console.WriteLine( e );
            return Reply<int>.None( ExceptionMessage );
        }
    }
    public async Task<Reply<int>> ExecuteStoredProcedure( string procedureName, DynamicParameters? parameters = null )
    {
        await using SqlConnection c = await GetOpenConnection();

        if (c.State != ConnectionState.Open)
            return Reply<int>.None( InvalidConnectionMessage + c.State );

        try {
            int result = await c.ExecuteAsync( procedureName, parameters, commandType: CommandType.StoredProcedure );
            return result > 0
                ? Reply<int>.With( result )
                : Reply<int>.None( "No Rows Altered" );
        }
        catch ( Exception e ) {
            Console.WriteLine( e );
            return Reply<int>.None( ExceptionMessage );
        }
    }
}