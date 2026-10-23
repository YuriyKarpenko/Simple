using Simple.Ttl;

namespace Test.Ttl;

public class ThreadTimerAsyncTests
{
    [Fact]
    public async Task SlowCallback_DoesNotOverlap()
    {
        //  arrange: the reentrancy guard must drop ticks while a callback is running
        var active = 0;
        var violations = 0;
        var runs = 0;

        //  test
        using (var timer = new ThreadTimerAsync(TimeSpan.FromMilliseconds(50), async _ =>
               {
                   if (Interlocked.Increment(ref active) > 1)
                   {
                       Interlocked.Increment(ref violations);
                   }
                   Interlocked.Increment(ref runs);
                   await Task.Delay(200);
                   Interlocked.Decrement(ref active);
               }))
        {
            await Task.Delay(600);
        }

        //  assert: never two callbacks at once, the loop keeps ticking
        Assert.Equal(0, Volatile.Read(ref violations));
        Assert.InRange(Volatile.Read(ref runs), 2, 5);
    }

    [Fact]
    public async Task CallbackException_DoesNotStopTimer()
    {
        //  arrange
        var attempts = 0;

        //  test
        using (var timer = new ThreadTimerAsync(TimeSpan.FromMilliseconds(50), _ =>
               {
                   Interlocked.Increment(ref attempts);
                   throw new InvalidOperationException("boom");
               }))
        {
            await Task.Delay(300);
        }

        //  assert: the catch inside ThreadTimerAsync keeps the loop alive
        Assert.InRange(Volatile.Read(ref attempts), 2, 10);
    }

    [Fact]
    public async Task Dispose_CancelsCallbackToken_AndIsIdempotent()
    {
        //  arrange: due=0, the first tick captures the callback token
        var seen = new CancellationToken[1];
        var timer = new ThreadTimerAsync(TimeSpan.FromMilliseconds(100), ct =>
        {
            seen[0] = ct;
            return Task.CompletedTask;
        });

        //  test
        await Task.Delay(150);
        timer.Dispose();
        timer.Dispose();

        //  assert
        Assert.True(seen[0].CanBeCanceled, "no tick received a token");
        Assert.True(seen[0].IsCancellationRequested);
    }
}
