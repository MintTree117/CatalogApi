using Microsoft.Extensions.Logging;
using OrderingDomain.Optionals;

namespace OrderingInfrastructure;

internal abstract class InfrastructureService<Tservice>( ILogger<Tservice> logger )
{
    protected const string DbNotSavedMessage = "Failed to save changes to database.";
    protected const string DbExceptionMessage = "An exception occured while accessing the database.";
    
    protected readonly ILogger<Tservice> Logger = logger;

    protected Reply<T> HandleDbException<T>( Exception e )
    {
        Logger.LogError( e, e.Message );
        return Reply<T>.None( DbExceptionMessage );
    }
    protected Replies<T> HandleDbExceptionOpts<T>( Exception e )
    {
        Logger.LogError( e, e.Message );
        return Replies<T>.None( DbExceptionMessage );
    }
}