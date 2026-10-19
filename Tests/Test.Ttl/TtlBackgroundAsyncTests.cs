using Simple.Ttl;

namespace Test.Ttl;

public class TtlBackgroundAsyncTests
{
    private static readonly TimeSpan ttl = TimeSpan.FromMilliseconds(100);

    [Fact]
    public async Task Ctor_PublishesInitialValue()
    {
        //  test
        using var svc = new TtlBackgroundAsync<int, int>(ttl, 0, (_, _) => Task.FromResult(7));

        //  assert
        Assert.Equal(7, await svc.Value);
    }

    [Fact]
    public async Task RefreshesValueInBackground()
    {
        //  arrange
        var calls = 0;
        using var svc = new TtlBackgroundAsync<int, int>(ttl, 0,
            async (_, _) =>
            {
                await Task.Yield();
                return Interlocked.Increment(ref calls);
            });

        //  test: ctor value + one tick per ttl
        await Task.Delay(TimeSpan.FromMilliseconds(ttl.TotalMilliseconds * 3.5));
        var actual = await svc.Value;

        //  assert
        Assert.InRange(actual, 2, 6);
    }

    [Fact]
    public async Task FailedRefresh_KeepsLastValue()
    {
        //  arrange: only successful results are published
        var calls = 0;
        using var svc = new TtlBackgroundAsync<int, int>(ttl, 0,
            async (_, _) =>
            {
                await Task.Yield();
                var n = Interlocked.Increment(ref calls);
                if (n > 1)
                {
                    throw new InvalidOperationException("boom");
                }
                return n;
            });

        //  test: at least two failed refreshes
        await Task.Delay(TimeSpan.FromMilliseconds(ttl.TotalMilliseconds * 2.5));

        //  assert: initial value survives, the timer loop survives
        Assert.Equal(1, await svc.Value);
    }

    [Fact]
    public async Task Dispose_StopsRefresh()
    {
        //  arrange
        var calls = 0;
        var svc = new TtlBackgroundAsync<int, int>(ttl, 0,
            (_, _) => Task.FromResult(Interlocked.Increment(ref calls)));

        //  test
        await Task.Delay(TimeSpan.FromMilliseconds(ttl.TotalMilliseconds * 1.5));
        svc.Dispose();
        await Task.Delay(50);               //  let a possible in-flight tick finish
        var snapshot = await svc.Value;
        var callsSnap = Volatile.Read(ref calls);

        await Task.Delay(TimeSpan.FromMilliseconds(ttl.TotalMilliseconds * 3));

        //  assert: no ticks after Dispose
        Assert.Equal(snapshot, await svc.Value);
        Assert.Equal(callsSnap, Volatile.Read(ref calls));
    }

    [Fact]
    public async Task Dispose_CancelsFactoryToken_AndIsIdempotent()
    {
        //  arrange: refresh ticks receive a cancelable token (the ctor call gets CancellationToken.None)
        var seen = new CancellationToken[1];
        var svc = new TtlBackgroundAsync<int, int>(ttl, 0, (_, ct) =>
        {
            if (ct.CanBeCanceled)
            {
                seen[0] = ct;
            }
            return Task.FromResult(0);
        });

        //  test
        await Task.Delay(TimeSpan.FromMilliseconds(ttl.TotalMilliseconds * 2.5));
        svc.Dispose();
        svc.Dispose();                      //  guarded by _disposed inside ThreadTimerAsync

        //  assert
        Assert.True(seen[0].CanBeCanceled, "no refresh tick received a token");
        Assert.True(seen[0].IsCancellationRequested);
    }

    [Fact]
    public async Task Dispose_LateResult_IsNotPublished()
    {
        //  arrange: refresh #2 is slow and outlives Dispose
        var calls = 0;
        var svc = new TtlBackgroundAsync<int, int>(ttl, 0, async (_, _) =>
        {
            var n = Interlocked.Increment(ref calls);
            if (n > 1)
            {
                await Task.Delay(400);
            }
            return n == 1 ? 1 : 99;
        });

        //  test: wait until the slow refresh is in flight, then dispose
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);
        while (Volatile.Read(ref calls) < 2 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }
        Assert.Equal(2, Volatile.Read(ref calls));  //  the slow refresh is in flight
        svc.Dispose();
        await Task.Delay(600);                      //  refresh #2 has finished by now

        //  assert: the late result must never be published
        Assert.Equal(1, await svc.Value);
    }

    [Fact]
    public async Task LastRefreshError_ReportsFailure_AndClearsOnSuccess()
    {
        //  arrange: ctor call ok, the first refresh fails, the next one succeeds
        var calls = 0;
        var svc = new TtlBackgroundAsync<int, int>(ttl, 0, (_, _) =>
        {
            var n = Interlocked.Increment(ref calls);
            if (n == 2)
            {
                throw new InvalidOperationException("boom");
            }
            return Task.FromResult(n);
        });

        //  test: wait for the failed tick, then for the successful one
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(3);
        while (svc.LastRefreshError == null && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }
        var failure = svc.LastRefreshError;
        while (svc.LastRefreshError != null && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }

        //  assert: the failure is reported, the next success clears it
        Assert.IsType<InvalidOperationException>(failure);
        Assert.Null(svc.LastRefreshError);
        svc.Dispose();
    }

    [Fact]
    public async Task Dispose_DuringCancelledRefresh_IsNotRecordedAsError()
    {
        //  arrange: the ctor call returns, the first refresh hangs until cancelled
        var calls = 0;
        var refreshStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var svc = new TtlBackgroundAsync<int, int>(ttl, 0, async (_, ct) =>
        {
            if (Interlocked.Increment(ref calls) == 1)
            {
                return 0;
            }
            refreshStarted.TrySetResult();
            await Task.Delay(Timeout.Infinite, ct);
            return 0;
        });

        //  test: dispose while the refresh factory is in flight
        await Task.WhenAny(refreshStarted.Task, Task.Delay(TimeSpan.FromSeconds(3)));
        Assert.True(refreshStarted.Task.IsCompleted, "the refresh tick must start within the deadline");
        svc.Dispose();
        await Task.Delay(100);

        //  assert: the cancellation on dispose is not a refresh failure
        Assert.Null(svc.LastRefreshError);
    }
}
