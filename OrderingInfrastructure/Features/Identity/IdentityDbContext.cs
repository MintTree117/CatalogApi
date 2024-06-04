using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderingDomain.Identity;

namespace OrderingInfrastructure.Features.Identity;

public sealed class IdentityDbContext : IdentityDbContext<UserAccount, UserRole, string>
{
    public IdentityDbContext( DbContextOptions<IdentityDbContext> options ) : base( options ) { }
    protected override void OnModelCreating( ModelBuilder builder )
    {
        base.OnModelCreating( builder );
        builder.Entity<UserAccount>().Property( u => u.Id ).ValueGeneratedOnAdd(); // Configure the table and primary key
    }
    public DbSet<UserAddress> Addresses { get; set; }
}