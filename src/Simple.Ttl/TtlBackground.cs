using System;
using System.Threading;
using System.Threading.Tasks;

namespace TF.Simple.Ttl;

public class TtlBackground<T> : IDisposable
{
    private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
    private readonly TimeSpan _ttl;
    private readonly Func<Task<T>> _factory;

    public TtlBackground(TimeSpan ttl, Func<Task<T>> factoryAsync)
    {
        _ttl = ttl;
        _factory = factoryAsync;
        Value = _factory();
        _ = Processing(_tokenSource.Token);
    }

    public Task<T> Value { get; private set; }

    public void Dispose()
    {
        _tokenSource.Cancel();
    }


    private async Task Processing(CancellationToken token)
    {
        while (await WaitAsync(_ttl, token))
        {
            var v = await _factory();
            Value = Task.FromResult(v);
        }
    }

    public static async Task<bool> WaitAsync(TimeSpan ttl, CancellationToken token)
    {
        await Task.Delay(ttl, token);
        return !token.IsCancellationRequested;
    }
}

#if NET6_0_OR_GREATER
public class TtlBackgroundTimer<T> : IDisposable
{
    private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
    private readonly PeriodicTimer _timer;
    private readonly Func<Task<T>> _factory;

    public TtlBackgroundTimer(TimeSpan ttl, Func<Task<T>> factoryAsync)
    {
        _timer = new PeriodicTimer(ttl);
        _factory = factoryAsync;
        Value = new ValueTask<T>(_factory());
        _ = Processing(_tokenSource.Token);
    }

    public ValueTask<T> Value { get; private set; }

    public void Dispose()
    {
        _tokenSource.Cancel();
        _timer.Dispose();
    }


    private async Task Processing(CancellationToken token)
    {
        while (await _timer.WaitForNextTickAsync(token))
        {
            var v = await _factory();
            Value = ValueTask.FromResult(v);
        }
    }
}
#endif