using Microsoft.AspNetCore.Identity;

namespace OrderingDomain.Identity;

// This is just so we can con figure IdentityDbContext; to change id type if we want to
public sealed class UserRole : IdentityRole<string> { }