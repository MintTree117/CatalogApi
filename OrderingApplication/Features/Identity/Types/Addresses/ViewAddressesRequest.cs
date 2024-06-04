namespace OrderingApplication.Features.Identity.Types.Addresses;

internal readonly record struct ViewAddressesRequest(
    int Page,
    int Results );