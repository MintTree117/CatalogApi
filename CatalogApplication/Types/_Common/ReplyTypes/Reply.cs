namespace CatalogApplication.Types._Common.ReplyTypes;

public readonly record struct Reply<T> : IReply
{
    public static bool operator true( Reply<T> reply ) => reply.Succeeded;
    public static bool operator false( Reply<T> reply ) => !reply.Succeeded;
    public static implicit operator bool( Reply<T> reply ) => reply.Succeeded;

    // Intentionally Unsafe: Up to programmer to keep track
    public readonly bool Succeeded;
    public T Data => _obj ?? throw new Exception( $"!!!!!!!!!!!! Fatal: Reply<{typeof( T )}>: Tried to access a null reply. !!!!!!!!!!!!" );
    public object GetData() => _obj ?? throw new Exception( $"!!!!!!!!!!!! Fatal: Reply<{typeof( T )}>: Tried to access a null reply. !!!!!!!!!!!!" );
    public string GetMessage() => _message ?? string.Empty;
    public bool CheckSuccess() => Succeeded;

    readonly T? _obj = default;
    readonly string? _message = null;
    
    public bool OutSuccess( out Reply<T> self )
    {
        self = this;
        return Succeeded;
    }
    public bool OutFailure( out Reply<T> self )
    {
        self = this;
        return !Succeeded;
    }
    
    public static Reply<T> Success( T obj ) => new( obj );
    public static Reply<T> Failure() => new();
    public static Reply<T> Failure( string msg ) => new( msg );
    public static Reply<T> Failure( IReply reply ) => new( reply.GetMessage() );
    public static Reply<T> Failure( IReply reply, string msg ) => new( $"{msg} {reply.GetMessage()}" );
    public static Reply<T> Failure( Exception ex ) => new( ex );
    public static Reply<T> Failure( Exception ex, string msg ) => new( ex, msg );

    public static Reply<T> NotFound() =>
        Failure( MsgNotFound );
    public static Reply<T> NotFound( string msg ) =>
        Failure( $"{MsgNotFound} {msg}" );
    public static Reply<T> NotFound( IReply other ) =>
        Failure( $"{MsgNotFound} {other.GetMessage()}" );

    public static Reply<T> UserNotFound() =>
        Failure( MsgUserNotFound );
    public static Reply<T> UserNotFound( string msg ) =>
        Failure( $"{MsgUserNotFound} {msg}" );
    public static Reply<T> UserNotFound( IReply other ) =>
        Failure( $"{MsgUserNotFound} {other.GetMessage()}" );

    public static Reply<T> Invalid() =>
        Failure( MsgValidationFailure );
    public static Reply<T> Invalid( string msg ) =>
        Failure( $"{MsgValidationFailure} {msg}" );
    public static Reply<T> Invalid( IReply other ) =>
        Failure( $"{MsgValidationFailure} {other.GetMessage()}" );

    public static Reply<T> InvalidPassword() =>
        Failure( MsgPasswordFailure );
    public static Reply<T> InvalidPassword( string msg ) =>
        Failure( $"{MsgPasswordFailure} {msg}" );
    public static Reply<T> InvalidPassword( IReply other ) =>
        Failure( $"{MsgPasswordFailure} {other.GetMessage()}" );

    public static Reply<T> ChangesNotSaved() =>
        Failure( MsgChangesNotSaved );
    public static Reply<T> ChangesNotSaved( string msg ) =>
        Failure( $"{MsgChangesNotSaved} {msg}" );
    public static Reply<T> ChangesNotSaved( IReply other ) =>
        Failure( $"{MsgChangesNotSaved} {other.GetMessage()}" );

    public static Reply<T> Conflict() =>
        Failure( MsgConflictError );
    public static Reply<T> Conflict( string msg ) =>
        Failure( $"{MsgConflictError} {msg}" );
    public static Reply<T> Conflict( IReply other ) =>
        Failure( $"{MsgConflictError} {other.GetMessage()}" );

    public static Reply<T> ServerError() =>
        Failure( MsgServerError );
    public static Reply<T> ServerError( string msg ) =>
        Failure( $"{MsgServerError} {msg}" );
    public static Reply<T> ServerError( IReply other ) =>
        Failure( $"{MsgServerError} {other.GetMessage()}" );

    public static Reply<T> Unauthorized() =>
        Failure( MsgUnauthorized );
    public static Reply<T> Unauthorized( string msg ) =>
        Failure( $"{MsgUnauthorized} {msg}" );
    public static Reply<T> Unauthorized( IReply other ) =>
        Failure( $"{MsgUnauthorized} {other.GetMessage()}" );
    
    public static Reply<T> BadRequest() =>
        Failure( MsgBadRequest );
    public static Reply<T> BadRequest( string msg ) =>
        Failure( $"{MsgBadRequest} {msg}" );
    public static Reply<T> BadRequest( IReply other ) =>
        Failure( $"{MsgBadRequest} {other.GetMessage()}" );

    const string MsgNotFound = "Request not found.";
    const string MsgUserNotFound = "User not found.";
    const string MsgValidationFailure = "Validation failed.";
    const string MsgPasswordFailure = "Invalid password.";
    const string MsgChangesNotSaved = "Failed to save changes to storage.";
    const string MsgConflictError = "A conflict has occured.";
    const string MsgServerError = "An internal server error occured.";
    const string MsgUnauthorized = "Unauthorized.";
    const string MsgBadRequest = "Bad Request.";

    Reply( T obj )
    {
        _obj = obj;
        Succeeded = true;
    }
    Reply( string? message = null ) =>
        _message = message;
    Reply( Exception e, string? message = null ) => 
        _message = $"{message} : Exception : {e} : {e.Message}";
}