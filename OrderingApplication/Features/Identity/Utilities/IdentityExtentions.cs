using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderingDomain.Identity;
using OrderingDomain.Optionals;

namespace OrderingApplication.Features.Identity.Utilities;

internal static class IdentityExtentions
{
    internal static async Task<Reply<UserAccount>> FindById( this UserManager<UserAccount> manager, string id )
    {
        UserAccount? user = await manager.Users.FirstOrDefaultAsync( u => u.Id == id );
        return user is not null
            ? Reply<UserAccount>.With( user )
            : Reply<UserAccount>.None( $"Unable to find user with id {id}." );
    }
    internal static async Task<Reply<UserAccount>> FindByEmail( this UserManager<UserAccount> manager, string email )
    {
        UserAccount? user = await manager.FindByEmailAsync( email );
        return user is not null
            ? Reply<UserAccount>.With( user )
            : Reply<UserAccount>.None( $"Unable to find user with email {email}." );
    }
    internal static async Task<Reply<UserAccount>> FindByEmailOrUsername( this UserManager<UserAccount> manager, string emailOrUsername )
    {
        UserAccount? user = await manager.Users.FirstOrDefaultAsync(
            c => c.Email == emailOrUsername || c.UserName == emailOrUsername );
        return user is not null
            ? Reply<UserAccount>.With( user )
            : Reply<UserAccount>.None( $"No account with username or email {emailOrUsername} was found." );
    }
    
    internal static string CombineErrors( this IdentityResult result )
    {
        StringBuilder builder = new();
        foreach ( IdentityError e in result.Errors )
            builder.Append( $"IdentityErrorCode: {e.Code} : Description: {e.Description}" );
        return builder.ToString();
    }
    internal static bool Succeeds( this IdentityResult result, out IdentityResult outResult )
    {
        outResult = result;
        return result.Succeeded;
    }
    internal static bool Fails( this IdentityResult result, out IdentityResult outResult )
    {
        outResult = result;
        return !result.Succeeded;
    }
    internal static bool Succeeds( this IdentityResult result ) => result.Succeeded;
    internal static bool Fails( this IdentityResult result ) => !result.Succeeded;
}