using OrderingDomain.Identity;
using OrderingDomain.ValueTypes;

namespace OrderingApplication.Features.Identity.Types.Addresses;

internal readonly record struct AddressDto(
    Guid Id,
    bool IsPrimary,
    string Country,
    string City,
    int GridX,
    int GridY )
{
    internal static IEnumerable<AddressDto> FromModels( IEnumerable<UserAddress> models )
    {
        List<AddressDto> dtos = [];
        dtos.AddRange( from m in models select FromModel( m ) );
        return dtos;
    }
    internal static AddressDto FromModel( UserAddress model ) =>
        new( model.Id, model.IsPrimary, model.Address.Country, model.Address.City, model.Address.GridX, model.Address.GridY );
    internal UserAddress ToModel( string identityId ) =>
        new( Id, identityId, new Address( Country, City, GridX, GridY ), IsPrimary );
}