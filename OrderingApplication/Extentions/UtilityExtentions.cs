using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OrderingDomain.Optionals;

namespace OrderingApplication.Extentions;

internal static class UtilityExtentions
{
    internal static string GetOrThrow( this IConfiguration configuration, string section ) =>
        configuration[section] ?? throw new Exception( $"Failed to get {section} from IConfiguration." );
    internal static Exception Exception( this IConfiguration configuration, string section ) =>
        new( $"Failed to get section {section} from IConfiguration." );
    
    internal static string UserId( this HttpContext context ) =>
        context.User.FindFirstValue( ClaimTypes.NameIdentifier ) ?? string.Empty;
    internal static string Email( this HttpContext context ) =>
        context.User.FindFirstValue( ClaimTypes.Email ) ?? string.Empty;
    internal static string Username( this HttpContext context ) =>
        context.User.FindFirstValue( ClaimTypes.Name ) ?? string.Empty;

    internal static IResult GetIResult<T>( this Reply<T> reply ) =>
        reply.IsSuccess
            ? Results.Ok( reply.Data )
            : FromOption( reply );
    internal static IResult GetIResult<T>( this Replies<T> reply ) =>
        reply.IsSuccess
            ? Results.Ok( reply.Enumerable )
            : FromOption( reply );
    static IResult FromOption( IReply reply ) =>
        Results.Problem( new ProblemDetails() {
            Detail = reply.Message()
        } );
}