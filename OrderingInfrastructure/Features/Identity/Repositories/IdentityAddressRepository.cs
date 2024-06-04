using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderingDomain.Identity;
using OrderingDomain.Optionals;

namespace OrderingInfrastructure.Features.Identity.Repositories;

internal sealed class IdentityAddressRepository( IdentityDbContext database, ILogger<IdentityAddressRepository> logger ) : InfrastructureService<IdentityAddressRepository>( logger ), IIdentityAddressRepository
{
    readonly IdentityDbContext db = database;

    public async Task<Reply<bool>> SaveAsync()
    {
        try {
            return await db.SaveChangesAsync() > 0
                ? Reply<bool>.With( true )
                : Reply<bool>.None( DbNotSavedMessage );
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<UserAddress>> GetAddress( Guid addressId )
    {
        try {
            UserAddress? result = await db.Addresses.FirstOrDefaultAsync( a => a.Id == addressId );
            return result is not null
                ? Reply<UserAddress>.With( result )
                : Reply<UserAddress>.None( $"No address found with id {addressId}." );
        }
        catch ( Exception e ) {
            return HandleDbException<UserAddress>( e );
        }
    }
    public async Task<Replies<UserAddress>> GetAllAddresses( string userId )
    {
        try {
            List<UserAddress> result =
                await db.Addresses
                        .Where( a => a.UserId == userId )
                        .ToListAsync();
            return Replies<UserAddress>.With( result );
        }
        catch ( Exception e ) {
            return HandleDbExceptionOpts<UserAddress>( e );
        }
    }
    public async Task<Reply<PagedResult<UserAddress>>> GetPagedAddresses( string userId, int page, int results )
    {
        try {
            int totalCount = await db.Addresses.CountAsync( a => a.UserId == userId );
            List<UserAddress> result =
                await db.Addresses
                        .Where( a => a.UserId == userId )
                        .Skip( results * GetPage( page ) )
                        .Take( results )
                        .ToListAsync();
            return Reply<PagedResult<UserAddress>>
                .With( PagedResult<UserAddress>
                    .With( totalCount, result ) );
        }
        catch ( Exception e ) {
            return HandleDbException<PagedResult<UserAddress>>( e );
        }

        static int GetPage( int page ) =>
            Math.Max( 0, page - 1 );
    }
    public async Task<Reply<bool>> AddAddress( UserAddress address )
    {
        try {
            await db.Addresses.AddAsync( address );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> UpdateAddress( UserAddress address )
    {
        try {
            UserAddress? model = await db.Addresses.FirstOrDefaultAsync( a => a.Id == address.Id );

            if (model is null)
                return IReply.None( "Address id not found." );

            model.Address = address.Address;
            model.IsPrimary = address.IsPrimary;
            
            db.Addresses.Update( model );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
    public async Task<Reply<bool>> DeleteAddress( UserAddress address )
    {
        try {
            db.Addresses.Remove( address );
            return await SaveAsync();
        }
        catch ( Exception e ) {
            return HandleDbException<bool>( e );
        }
    }
}