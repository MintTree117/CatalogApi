namespace CatalogApplication.Types._Common.ReplyTypes;

public interface IReply
{
    public bool CheckSuccess();
    public string GetMessage();
    public object GetData();

    public static Reply<bool> Success() => Reply<bool>.Success( true );
    public static Reply<bool> Fail() => Reply<bool>.Failure();
    public static Reply<bool> Fail( string msg ) => Reply<bool>.Failure( msg );
    public static Reply<bool> Fail( IReply other ) => Reply<bool>.Failure( other );
    public static Reply<bool> Fail( IReply other, string msg ) => Reply<bool>.Failure( other, msg );
    public static Reply<bool> Fail( Exception ex ) => Reply<bool>.Failure( ex );
    public static Reply<bool> Fail( Exception ex, string msg ) => Reply<bool>.Failure( ex, msg );

    public bool OutSuccess( out IReply self )
    {
        self = this;
        return CheckSuccess();
    }
    public bool OutFailure( out IReply self )
    {
        self = this;
        return !CheckSuccess();
    }

    public static Reply<bool> NotFound() =>
        Reply<bool>.Failure( MsgNotFound );
    public static Reply<bool> NotFound(string msg) =>
        Reply<bool>.Failure($"{MsgNotFound} {msg}");
    public static Reply<bool> NotFound(IReply other) =>
        Reply<bool>.Failure($"{MsgNotFound} {other.GetMessage()}");

    public static Reply<bool> UserNotFound() =>
        Reply<bool>.Failure( MsgUserNotFound );
    public static Reply<bool> UserNotFound(string msg) =>
        Reply<bool>.Failure($"{MsgUserNotFound} {msg}");
    public static Reply<bool> UserNotFound(IReply other) =>
        Reply<bool>.Failure($"{MsgUserNotFound} {other.GetMessage()}");

    public static Reply<bool> Invalid() =>
        Reply<bool>.Failure( MsgValidationFailure );
    public static Reply<bool> Invalid(string msg) =>
        Reply<bool>.Failure($"{MsgValidationFailure} {msg}");
    public static Reply<bool> Invalid(IReply other) =>
        Reply<bool>.Failure($"{MsgValidationFailure} {other.GetMessage()}");

    public static Reply<bool> InvalidPassword() =>
        Reply<bool>.Failure( MsgPasswordFailure );
    public static Reply<bool> InvalidPassword(string msg) =>
        Reply<bool>.Failure($"{MsgPasswordFailure} {msg}");
    public static Reply<bool> InvalidPassword(IReply other) =>
        Reply<bool>.Failure($"{MsgPasswordFailure} {other.GetMessage()}");

    public static Reply<bool> ChangesNotSaved() =>
        Reply<bool>.Failure( MsgChangesNotSaved );
    public static Reply<bool> ChangesNotSaved(string msg) =>
        Reply<bool>.Failure($"{MsgChangesNotSaved} {msg}");
    public static Reply<bool> ChangesNotSaved(IReply other) =>
        Reply<bool>.Failure($"{MsgChangesNotSaved} {other.GetMessage()}");

    public static Reply<bool> Conflict() =>
        Reply<bool>.Failure( MsgConflictError );
    public static Reply<bool> Conflict(string msg) =>
        Reply<bool>.Failure($"{MsgConflictError} {msg}");
    public static Reply<bool> Conflict(IReply other) =>
        Reply<bool>.Failure($"{MsgConflictError} {other.GetMessage()}");

    public static Reply<bool> ServerError() =>
        Reply<bool>.Failure( MsgServerError );
    public static Reply<bool> ServerError(string msg) =>
        Reply<bool>.Failure($"{MsgServerError} {msg}");
    public static Reply<bool> ServerError(IReply other) =>
        Reply<bool>.Failure($"{MsgServerError} {other.GetMessage()}");
        
    public static Reply<bool> Unauthorized() =>
        Fail( MsgUnauthorized );
    public static Reply<bool> Unauthorized( string msg ) =>
        Fail( $"{MsgUnauthorized} {msg}" );
    public static Reply<bool> Unauthorized( IReply other ) =>
        Fail( $"{MsgUnauthorized} {other.GetMessage()}" );

    public static Reply<bool> BadRequest() =>
        Fail( MsgBadRequest );
    public static Reply<bool> BadRequest( string msg ) =>
        Fail( $"{MsgBadRequest} {msg}" );
    public static Reply<bool> BadRequest( IReply other ) =>
        Fail( $"{MsgBadRequest} {other.GetMessage()}" );
    
    public const string MsgUnauthorized = "Unauthorized.";
    public const string MsgNotFound = "Request not found.";
    public const string MsgUserNotFound = "User not found.";
    public const string MsgValidationFailure = "Validation failed.";
    public const string MsgPasswordFailure = "Invalid password.";
    public const string MsgChangesNotSaved = "Failed to save changes to storage.";
    public const string MsgConflictError = "A conflict has occured.";
    public const string MsgServerError = "An internal server error occured.";
    public const string MsgBadRequest = "Bad Request.";
}