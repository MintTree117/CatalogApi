using CatalogApplication.Types._Common.ReplyTypes;

namespace CatalogApplication.Repositories;

internal sealed class MemoryCache<Tmemory,Tlogger> : IDisposable // Singleton
{
    readonly Timer _timer;
    readonly ILogger<Tlogger> _logger;
    readonly SemaphoreSlim _semaphore = new( 1, 1 );
    Reply<Tmemory> _data = Reply<Tmemory>.Failure();
    bool _disposed = false;

    public void Dispose()
    {
        if (_disposed)
            return;
        _timer.Dispose();
        _semaphore.Dispose();
        _disposed = true;
    }
    public MemoryCache( TimeSpan timeSpan, Func<Task<Reply<Tmemory>>> refreshCallback, ILogger<Tlogger> logger )
    {
        _logger = logger;
        _timer = new Timer( TimerCallback, null, Timeout.InfiniteTimeSpan, timeSpan );
        _timer.Change( TimeSpan.Zero, timeSpan );

        async void TimerCallback( object? state )
        {
            await ExecuteRefreshCallback( refreshCallback );
        }
    }
    internal async Task<Reply<Tmemory>> Get()
    {
        await _semaphore.WaitAsync();

        try
        {
            return _data;
        }
        finally
        {
            _semaphore.Release();
        }
    }
    async Task ExecuteRefreshCallback( Func<Task<Reply<Tmemory>>> callback )
    {
        await _semaphore.WaitAsync();

        try
        {
            Reply<Tmemory> callbackReply = await callback.Invoke();
            if (!callbackReply)
            {
                _logger.LogError( $"An error occured during cache callback. {callbackReply.GetMessage()}" );
                return;
            }

            _data = Reply<Tmemory>.Success( callbackReply.Data );
            _logger.LogInformation( $"MemoryCache {nameof( Tmemory )} Refreshed." );
        }
        catch ( Exception e )
        {
            _logger.LogError( e, "An exception occured during cache callback." );
        }
        finally
        {
            _semaphore.Release();
        }
    }
}