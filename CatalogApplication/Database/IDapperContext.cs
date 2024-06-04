using CatalogApplication.Types.ReplyTypes;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CatalogApplication.Database;

internal interface IDapperContext
{
    internal Task<SqlConnection> GetOpenConnection();
    internal Task<Replies<T>> QueryAsync<T>( string sql, DynamicParameters? parameters = null );
    internal Task<Reply<T>> QueryFirstOrDefaultAsync<T>( string sql, DynamicParameters? parameters = null );
}