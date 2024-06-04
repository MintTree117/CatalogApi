using Microsoft.EntityFrameworkCore;
using OrderingDomain.Orders;

namespace OrderingInfrastructure.Features.Ordering;

internal sealed class OrderingDbContext : DbContext
{
    public OrderingDbContext( DbContextOptions<OrderingDbContext> options ) : base( options ) { }

    public DbSet<OrderStateDelayTime> DelayTimes { get; set; }
    public DbSet<OrderStateExpireTime> ExpireTimes { get; set; }

    public DbSet<OrderProblem> OrderProblems { get; set; }
    public DbSet<OrderLine> PendingCancelOrderLines { get; set; }
    public DbSet<Order> ActiveOrders { get; set; }
    public DbSet<OrderLine> ActiveOrderLines { get; set; }
    public DbSet<OrderItem> ActiveOrderItems { get; set; }
    public DbSet<Order> InActiveOrders { get; set; }
    public DbSet<OrderLine> InActiveOrderGroups { get; set; }
    public DbSet<OrderItem> InActiveOrderItems { get; set; }
    public DbSet<OrderLocation> WarehouseLocations { get; set; }
}