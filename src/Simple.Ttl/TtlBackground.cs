using System;
using System.Threading;

namespace Simple.Ttl;

/// <summary> Background value with periodic refresh: the factory runs on a timer, callers only read
/// <see cref="Value"/>; refresh exceptions never reach the thread pool. </summary>
public class TtlBackground<TArg, T> : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly CancellationToken       _ct;
    private readonly TArg                    _arg;
    private readonly Func<TArg, T>           _factory;
    private readonly Timer                   _timer;

    //  atomic reference publication: no torn reads for structs, no stale reads for references
    private object? _value;
    private int     _running;

    /// <summary> Creates the value once with <paramref name="factory"/>, then refreshes it every <paramref name="ttl"/>. </summary>
    public TtlBackground(TimeSpan ttl, TArg arg, Func<TArg, T> factory)
    {
        _ct      = _cts.Token;
        _arg     = arg;
        _factory = factory;
        _value   = _factory(arg);
        _timer   = new Timer(Processing, null, ttl, ttl);
    }

    /// <summary> The latest published value; safe to read from any thread (atomic reference read). </summary>
    public T Value => (T)Volatile.Read(ref _value)!;

    private Exception? _lastError;

    /// <summary> The failure of the last refresh; null before the first failure and after a successful refresh. </summary>
    public Exception? LastRefreshError => Volatile.Read(ref _lastError);

    private bool _disposed;

    /// <summary> Stops the timer and cancels the token; idempotent. </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _timer.Dispose();
            _cts.Cancel();
            _cts.Dispose();
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }


    private void Processing(object? _)
    {
        //  reentrancy guard: a factory longer than the period must not start a parallel tick
        if (_ct.IsCancellationRequested || Interlocked.CompareExchange(ref _running, 1, 0) != 0)
        {
            return;
        }

        try
        {
            Volatile.Write(ref _value, _factory(_arg));
            Volatile.Write(ref _lastError, null);
        }
        catch (Exception ex)
        {
            Volatile.Write(ref _lastError, ex);
        }
        finally
        {
            Interlocked.Exchange(ref _running, 0);
        }
    }
}