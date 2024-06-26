using CatalogApplication.Database;
using CatalogApplication.Types._Common.ReplyTypes;
namespace CatalogApplication.Repositories;

internal abstract class BaseRepository<T>( IDapperContext dapper, ILogger<T> logger )
{
    protected readonly ILogger<T> Logger = logger;
    protected readonly IDapperContext Dapper = dapper;

    protected void LogIfErrorReply( IReply reply )
    {
        if (!reply.CheckSuccess())
            Logger.LogError( reply.GetMessage() );
    }
    protected void LogInformation( string message ) =>
        Logger.LogInformation( message );
    protected void LogError( string message ) =>
        Logger.LogError( message );
    protected void LogException( Exception exception, string message ) =>
        Logger.LogError( exception, message );
    protected void LogInfoOrError( bool condition, string infoMessage, string errorMessage )
    {
        if (condition) Logger.LogInformation( infoMessage );
        else Logger.LogError( errorMessage );
    }

    protected Reply<Tlog> LogIfErrorReplyReturn<Tlog>( Reply<Tlog> reply )
    {
        LogIfErrorReply( reply );
        return reply;
    }
    protected IReply LogIfErrorReplyReturn( IReply reply )
    {
        LogIfErrorReply( reply );
        return reply;
    }
}