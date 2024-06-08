using CatalogApplication.Types.ReplyTypes;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Database;

internal interface IDapperContext
{
    internal static IDapperContext GetContext( IServiceProvider provider )
    {
        using AsyncServiceScope scope = provider.CreateAsyncScope();
        return scope.ServiceProvider.GetService<DapperContext>() ?? throw new Exception( "Failed to get DapperContext" );
    }
    internal Task<SqlConnection> GetOpenConnection();
    internal Task<Replies<T>> QueryAsync<T>( string sql, DynamicParameters? parameters = null );
    internal Task<Reply<T>> QueryFirstOrDefaultAsync<T>( string sql, DynamicParameters? parameters = null );
    internal Task<Reply<int>> ExecuteAsync( string sql, DynamicParameters? parameters = null );
    internal Task<Reply<int>> ExecuteStoredProcedure( string procedureName, DynamicParameters? parameters = null );
}