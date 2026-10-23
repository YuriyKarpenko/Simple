using Simple.Ttl;

namespace Test.Ttl;

public class TtlBackgroundTests
{
    private static readonly TimeSpan ttl = TimeSpan.FromMilliseconds(100);

    [Theory]
    [InlineData(10, 5)]
    [InlineData(10, 10)]
    [InlineData(10, 25)]
    [InlineData(10, 50)]
    public async Task TaestAsync(int expectedIterations, int factoryDelayMls)
    {
        //  arange
        var waiting = ttl * expectedIterations;
        var calcTime = TimeSpan.FromMilliseconds(factoryDelayMls);

        //  ctor value + one tick per ttl (due = ttl, anchored schedule)
        var expected = (int)Math.Round(waiting / ttl) + 1;

        var runCount = 0;
        int factory(int _)
        {
            Thread.Sleep(calcTime);
            return Interlocked.Increment(ref runCount);
        }

        var svc = new TtlBackground<int, int>(ttl, 0, factory);
        await Task.Delay(waiting);

        //  test
        var actual = svc.Value;

        //  assert: timer granularity may shift the count by one in either direction
        Assert.InRange(actual, expected - 1, expected + 1);
    }

    [Fact]
    public void FailedRefresh_KeepsLastValue()
    {
        //  arrange: timer callback swallows factory exceptions (try/catch in Processing)
        var calls = 0;
        var svc = new TtlBackground<int, int>(ttl, 0, _ =>
        {
            var n = Interlocked.Increment(ref calls);
            if (n > 1)
            {
                throw new InvalidOperationException("boom");
            }
            return n;
        });

        //  test
        Thread.Sleep(250);

        //  assert: ctor value survives failed refreshes
        Assert.Equal(1, svc.Value);
        svc.Dispose();
    }

    [Fact]
    public void Dispose_StopsRefresh()
    {
        //  arrange
        var calls = 0;
        var svc = new TtlBackground<int, int>(ttl, 0, _ => Interlocked.Increment(ref calls));

        //  test
        Thread.Sleep(150);
        svc.Dispose();
        Thread.Sleep(30);                   //  let a possible in-flight tick finish
        var snapshot = svc.Value;
        var callsSnap = Volatile.Read(ref calls);

        Thread.Sleep(300);

        //  assert: no ticks after Dispose
        Assert.Equal(snapshot, svc.Value);
        Assert.Equal(callsSnap, Volatile.Read(ref calls));
    }

    [Fact]
    public void SlowFactory_DoesNotOverlap()
    {
        //  arrange: the reentrancy guard must drop ticks while the factory is running
        var active = 0;
        var violations = 0;
        var runs = 0;
        var svc = new TtlBackground<int, int>(ttl, 0, _ =>
        {
            if (Interlocked.Increment(ref active) > 1)
            {
                Interlocked.Increment(ref violations);
            }
            Interlocked.Increment(ref runs);
            Thread.Sleep(250);
            Interlocked.Decrement(ref active);
            return 1;
        });

        //  test: ttl = 100ms vs factory = 250ms
        Thread.Sleep(700);
        svc.Dispose();

        //  assert: never two factories at once, the loop keeps ticking
        Assert.Equal(0, Volatile.Read(ref violations));
        Assert.InRange(Volatile.Read(ref runs), 2, 4);
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        //  arrange
        var svc = new TtlBackground<int, int>(ttl, 0, _ => 1);

        //  test: the second call must not throw (IDisposable contract)
        var first = Record.Exception(svc.Dispose);
        var second = Record.Exception(svc.Dispose);

        //  assert
        Assert.Null(first);
        Assert.Null(second);
    }

    [Fact]
    public void LastRefreshError_ReportsFailure_AndClearsOnSuccess()
    {
        //  arrange: ctor call ok, the first refresh fails, the next one succeeds
        var calls = 0;
        var svc = new TtlBackground<int, int>(ttl, 0, _ =>
        {
            var n = Interlocked.Increment(ref calls);
            if (n == 2)
            {
                throw new InvalidOperationException("boom");
            }
            return n;
        });

        //  test: wait for the failed tick, then for the successful one
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(3);
        while (svc.LastRefreshError == null && DateTime.UtcNow < deadline)
        {
            Thread.Sleep(10);
        }
        var failure = svc.LastRefreshError;
        while (svc.LastRefreshError != null && DateTime.UtcNow < deadline)
        {
            Thread.Sleep(10);
        }

        //  assert: the failure is reported, the next success clears it
        Assert.IsType<InvalidOperationException>(failure);
        Assert.Null(svc.LastRefreshError);
        svc.Dispose();
    }
}