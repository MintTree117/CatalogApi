namespace OrderingApplication.Features.Identity.Types.Addresses;

internal readonly record struct ViewAddressesReply(
    int TotalCount,
    IEnumerable<AddressDto> Addresses )
{
    internal static ViewAddressesReply With( int count, IEnumerable<AddressDto> addresses ) =>
        new( count, addresses );
}