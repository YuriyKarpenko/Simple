using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Ttl;

/// <summary> Periodic async timer with a reentrancy guard: overlapping ticks are dropped, callback
/// exceptions never reach the thread pool, the token is cancelled on dispose. </summary>
public class ThreadTimerAsync : IDisposable
{
    private readonly CancellationTokenSource       _cts = new();
    private readonly CancellationToken             _ct;
    private readonly Func<CancellationToken, Task> _onTimeAsync;
    private readonly Timer                         _timer;
    private          int                           _running;

    /// <summary> Schedules <paramref name="onTimeAsync"/> every <paramref name="ttl"/>,
    /// starting after <paramref name="delay"/> (default - immediately). </summary>
    public ThreadTimerAsync(TimeSpan ttl, Func<CancellationToken, Task> onTimeAsync, TimeSpan? delay = null)
    {
        _onTimeAsync = onTimeAsync ?? throw new ArgumentNullException(nameof(onTimeAsync));
        _ct          = _cts.Token;
        _timer       = new Timer(Tick, null, delay ?? TimeSpan.Zero, ttl);
    }

    //  timer callback is synchronous: ProcessingAsync never throws - an exception must not escape onto the ThreadPool
    private void Tick(object? state)
        => _ = ProcessingAsync(state);

    private async Task ProcessingAsync(object? _)
    {
        var ct = _ct;
        if (ct.IsCancellationRequested || Interlocked.CompareExchange(ref _running, 1, 0) != 0)
        {
            return;
        }

        try
        {
            await _onTimeAsync(ct);
        }
        catch
        {
            //  disposed during the factory call - not a refresh failure
        }
        finally
        {
            Interlocked.Exchange(ref _running, 0);
        }
    }

    #region IDisposable

    private bool _disposed;
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _cts.Cancel();
                _timer.Dispose();
                _cts.Dispose();
            }

            _disposed = true;
        }
    }

    /// <summary> Cancels the token and stops the timer; idempotent. </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

}