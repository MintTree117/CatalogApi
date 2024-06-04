using OrderingDomain.Optionals;
using OrderingDomain.Orders;
using OrderingInfrastructure.Email;
using OrderingInfrastructure.Features.Ordering.Repositories;

namespace OrderingApplication.Features.Ordering.Services;

internal sealed class OrderStateFlaggingService( IServiceProvider serviceProvider, IConfiguration configuration ) : BackgroundService
{
    readonly IServiceProvider _serviceProvider = serviceProvider;
    readonly OrderStateFlaggingConfiguration _config = GetConfig( configuration );
    readonly Dictionary<OrderState, TimeSpan> _orderDelayTimespans = [];
    readonly Dictionary<OrderState, TimeSpan> _orderExpireTimespans = [];

    protected override async Task ExecuteAsync( CancellationToken stoppingToken )
    {
        using PeriodicTimer timer = new( _config.RefreshInterval );
        try {
            while ( await timer.WaitForNextTickAsync( stoppingToken ) )
                await Handle();
        }
        catch ( OperationCanceledException ) {
            Console.WriteLine( "Timed Hosted Service is stopping." );
        }
    }

    async Task Handle()
    {
        if (!ValidTime( _config ))
            return;
        await using AsyncServiceScope scope = _serviceProvider.CreateAsyncScope();
        var services = Services.Create( scope );
        await HandleDelayedOrderLines( services );
        await HandleExpiredOrderLines( services );
    }
    async Task HandleDelayedOrderLines( Services s )
    {
        if ((await GetOrderDelayedTimeSpans( s )).Fails( out var opt )) {
            Console.WriteLine( opt.Message() );
            return;
        }

        if ((await s.UtilRepo.GetTopUnhandledDelayedOrderLines( _config.MaxDelayedIterationsPerExecution, 8 ))
            .Fail( out Replies<OrderLine> ordersOpt ))
            Console.WriteLine( ordersOpt.Message() );

        foreach ( OrderLine o in ordersOpt.Enumerable )
            await HandleDelayedOrderLines( o, s );
    }
    async Task HandleExpiredOrderLines( Services s )
    {
        if ((await GetOrderExpiredTimeSpans( s )).Fails( out var opt )) {
            Console.WriteLine( opt.Message() );
            return;
        }

        if ((await s.UtilRepo.GetTopUnhandledExpiredOrderLines( _config.MaxExpiredIterationsPerExecution, 8 ))
            .Fail( out Replies<OrderLine> ordersOpt ))
            Console.WriteLine( ordersOpt.Message() );

        foreach ( OrderLine o in ordersOpt.Enumerable )
            await HandleExpiredOrderLines( o, s );
    }
    async Task HandleDelayedOrderLines( OrderLine o, Services s )
    {
        if (!DateExpired( o.LastUpdate, _orderDelayTimespans[o.State] ))
            return;

        o.Delayed = true;
        if ((await s.UtilRepo.SaveAsync()).Fails( out Reply<bool> saveResult ))
            Console.WriteLine( saveResult.Message() );

        await NotifyCustomerOfDelay( o, s );
    }
    async Task HandleExpiredOrderLines( OrderLine o, Services s )
    {
        if (!DateExpired( o.LastUpdate, _orderExpireTimespans[o.State] ))
            return;
        OrderProblem problem = new();
        await s.UtilRepo.InsertOrderProblem( problem );
        await NotifyCustomerOfProblem( problem, s );
    }

    static bool ValidTime( OrderStateFlaggingConfiguration c ) =>
        DateTime.Now.Hour >= c.ServiceStartHour || DateTime.Now.Hour < c.ServiceEndHour;
    static bool DateExpired( DateTime date, TimeSpan time ) =>
        DateTime.Now - date < time;

    static async Task NotifyCustomerOfDelay( OrderLine o, Services s )
    {
        if ((await s.Repo.GetOrderById( o.OrderId )).Fails( out Reply<Order> order )) {
            Console.WriteLine( order.Message() );
            return;
        }
        s.Email.SendBasicEmail( order.Data.CustomerEmail, "Order Line Delayed", $"Unfortunately, your order line {o.Id} of order {order.Data.Id} has been delayed. It is currently {o.State}" );
    }
    static async Task NotifyCustomerOfProblem( OrderProblem problem, Services s )
    {
        if ((await s.Repo.GetOrderById( problem.OrderId )).Fails( out Reply<Order> order )) {
            Console.WriteLine( order.Message() );
            return;
        }
        s.Email.SendBasicEmail( order.Data.CustomerEmail, "Order Problem Encountered", $"Unfortunately, a problem has occured for order line {problem.OrderLineId} of order {problem.OrderId}. An admin has been notified. Please contact support for more information." );
    }

    static OrderStateFlaggingConfiguration GetConfig( IConfiguration configuration )
    {
        OrderStateFlaggingConfiguration c = new();
        IConfigurationSection orderStateFlaggingSection = configuration.GetSection( "Ordering:OrderStateFlaggingService" );
        c.RefreshInterval = TimeSpan.Parse( orderStateFlaggingSection["RefreshInterval"] ?? "01:00:00" );
        orderStateFlaggingSection.Bind( c );
        return c;
    }
    async Task<Reply<bool>> GetOrderDelayedTimeSpans( Services services )
    {
        if ((await services.UtilRepo.GetDelayTimes()).Fail( out Replies<OrderStateDelayTime> delays ))
            return IReply.None( delays.Message() );

        _orderDelayTimespans.Clear();
        foreach ( OrderStateDelayTime delayTime in delays.Enumerable )
            _orderDelayTimespans.TryAdd( delayTime.State, delayTime.DelayTime );

        return IReply.Okay();
    }
    async Task<Reply<bool>> GetOrderExpiredTimeSpans( Services services )
    {
        if ((await services.UtilRepo.GetExpiryTimes()).Fail( out Replies<OrderStateExpireTime> expires ))
            return IReply.None( expires.Message() );

        _orderExpireTimespans.Clear();
        foreach ( OrderStateExpireTime expireTIme in expires.Enumerable )
            _orderExpireTimespans.TryAdd( expireTIme.State, expireTIme.ExpiryTime );

        return IReply.Okay();
    }

    sealed class OrderStateFlaggingConfiguration
    {
        public TimeSpan RefreshInterval { get; set; }
        public int ServiceStartHour { get; set; }
        public int ServiceEndHour { get; set; }
        public int MaxDelayedIterationsPerExecution { get; set; }
        public int MaxExpiredIterationsPerExecution { get; set; }
    }
    sealed class Services
    {
        public static Services Create( AsyncServiceScope scope ) =>
            new() {
                Email = scope.ServiceProvider.GetService<IEmailSender>()!,
                Repo = scope.ServiceProvider.GetService<IOrderingRepository>()!,
                UtilRepo = scope.ServiceProvider.GetService<IOrderingUtilityRepository>()!
            };
        public IEmailSender Email = null!;
        public IOrderingRepository Repo = null!;
        public IOrderingUtilityRepository UtilRepo = null!;
    }
}