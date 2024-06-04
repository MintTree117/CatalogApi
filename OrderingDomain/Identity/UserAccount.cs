using Microsoft.AspNetCore.Identity;

namespace OrderingDomain.Identity;

public sealed class UserAccount : IdentityUser<string>
{
    public UserAccount() : base() { }
    public UserAccount( string email, string username ) : base()
    {
        Email = email;
        UserName = username;
    }

    public ICollection<UserAddress> Addresses { get; set; } = [];
}