using OrderingApplication.Features.Identity.Types.Addresses;
using OrderingDomain.Identity;
using OrderingDomain.Optionals;
using OrderingInfrastructure;
using OrderingInfrastructure.Features.Identity.Repositories;

namespace OrderingApplication.Features.Identity.Services.Account;

internal sealed class AddressSystem( IIdentityAddressRepository addressRepository )
{
    readonly IIdentityAddressRepository repo = addressRepository;

    internal async Task<Reply<bool>> AddAddress( string userId, AddressDto request )
    {
        UserAddress model = request.ToModel( userId );

        Replies<UserAddress> any = await repo.GetAllAddresses( userId );
        if (!any.IsSuccess)
            return IReply.None( any );

        if (!any.Enumerable.Any())
            model.IsPrimary = true;
        
        return await repo.AddAddress( model );
    }
    internal async Task<Reply<bool>> UpdateAddress( string userId, AddressDto request )
    {
        UserAddress model = request.ToModel( userId );

        // Ensure there can only be one primary address at a time
        Replies<UserAddress> otherPrimaryAddresses = await repo.GetAllAddresses( userId );
        if (model.IsPrimary) {
            foreach ( UserAddress otherAddress in otherPrimaryAddresses.Enumerable ) {
                // Set all other addresses to not be primary
                otherAddress.IsPrimary = false;
                await repo.UpdateAddress( otherAddress );
            }
        }
        else if (!otherPrimaryAddresses.Enumerable.Any( a => a.IsPrimary ))
                return IReply.None( "There must be at least one primary address." );
        
        return await repo.UpdateAddress( model );
    }
    internal async Task<Reply<bool>> DeleteAddress( string userId, Guid addressId )
    {
        Reply<UserAddress> address = await repo.GetAddress( addressId );

        if (!address.IsSuccess)
            return IReply.None( address );
        
        bool wasPrimary = address.Data.IsPrimary;
        Reply<bool> deleteResult = await repo.DeleteAddress( address.Data );

        if (!deleteResult.IsSuccess)
            return IReply.None( deleteResult );

        if (!wasPrimary)
            return IReply.Okay();

        Replies<UserAddress> addresses = await repo.GetAllAddresses( userId );
        if (!addresses.Enumerable.Any())
            return IReply.Okay();

        UserAddress newPrimary = addresses.Enumerable.First();
        newPrimary.IsPrimary = true;

        await repo.UpdateAddress( newPrimary );
        return IReply.Okay(); // Its still success if deleted but not updated
    }
    internal async Task<Reply<ViewAddressesReply>> ViewAddresses( string userId, ViewAddressesRequest request )
    {
        Reply<PagedResult<UserAddress>> result = await repo.GetPagedAddresses( userId, request.Page, request.Results );
        IEnumerable<AddressDto> dtos = AddressDto.FromModels( result.Data.Items );
        return result.IsSuccess
            ? Reply<ViewAddressesReply>.With( ViewAddressesReply.With( result.Data.TotalCount, dtos ) )
            : Reply<ViewAddressesReply>.None( result );
    }
}