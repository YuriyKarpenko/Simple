using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Ttl;

/// <summary> Async background value: the factory receives a token that is cancelled on dispose,
/// only successful results are published to <see cref="Value"/>. </summary>
public class TtlBackgroundAsync<TArg, TRes> : IDisposable
{
    private readonly TArg                                      _arg;
    private readonly Func<TArg, CancellationToken, Task<TRes>> _factory;
    private readonly ThreadTimerAsync                          _timer;

    private Task<TRes> _task;
    private Exception? _lastError;

    /// <summary> Runs the factory once for the initial value, then every <paramref name="ttl"/> with the live token. </summary>
    public TtlBackgroundAsync(TimeSpan ttl, TArg arg, Func<TArg, CancellationToken, Task<TRes>> factoryAsync)
    {
        _arg     = arg;
        _factory = factoryAsync ?? throw new ArgumentNullException(nameof(factoryAsync));
        _task    = _factory(arg, CancellationToken.None);
        _timer   = new ThreadTimerAsync(ttl, ProcessingAsync, ttl);
    }

    /// <summary> Task of the latest published value; read it with await. </summary>
    public Task<TRes> Value => Volatile.Read(ref _task);

    /// <summary> The failure of the last refresh; null before the first failure and after a successful refresh. </summary>
    public Exception? LastRefreshError => Volatile.Read(ref _lastError);

    /// <summary> Cancels the token and stops further refreshes; idempotent. </summary>
    public void Dispose()
        => _timer.Dispose();

    private async Task ProcessingAsync(CancellationToken ct)
    {
        //  publish only a successful result - a failed refresh keeps the previous value
        try
        {
            var v = await _factory(_arg, ct);
            if (ct.IsCancellationRequested)
            {
                return; //  disposed while the factory was running - a late result must not be published
            }
            Volatile.Write(ref _task, Task.FromResult(v));
            Volatile.Write(ref _lastError, null);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            //  disposed during the factory call - not a refresh failure
        }
        catch (Exception ex)
        {
            Volatile.Write(ref _lastError, ex);
        }
    }
}