using OrderingDomain.ValueTypes;

namespace OrderingApplication.Features.Ordering.Dtos;

public static class AddressExtentions
{
    public static HeuristicDistance HeuristicDistanceFrom( this Address address, Address other )
    {
        return new HeuristicDistance();
    }
}