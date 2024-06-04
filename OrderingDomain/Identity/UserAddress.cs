using OrderingDomain._Common;
using OrderingDomain.ValueTypes;

namespace OrderingDomain.Identity;

public sealed class UserAddress : IEntity
{
    public UserAddress() { }
    public UserAddress( Guid id, string userId, Address address, bool isPrimary )
    {
        Id = id;
        UserId = userId;
        Address = address;
        IsPrimary = isPrimary;
    }

    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Address Address { get; set; }
    public bool IsPrimary { get; set; }
}