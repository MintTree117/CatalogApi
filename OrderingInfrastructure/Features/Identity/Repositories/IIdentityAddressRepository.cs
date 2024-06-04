using OrderingDomain.Identity;
using OrderingDomain.Optionals;

namespace OrderingInfrastructure.Features.Identity.Repositories;

public interface IIdentityAddressRepository
{
    public Task<Reply<UserAddress>> GetAddress( Guid addressId );
    public Task<Replies<UserAddress>> GetAllAddresses( string userId );
    public Task<Reply<PagedResult<UserAddress>>> GetPagedAddresses( string userId, int page, int results );
    public Task<Reply<bool>> AddAddress( UserAddress address );
    public Task<Reply<bool>> UpdateAddress( UserAddress address );
    public Task<Reply<bool>> DeleteAddress( UserAddress address );
}