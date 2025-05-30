using TF.Simple.Ttl;

namespace Test.Ttl;

public class TtlBackgroundTests
{
    private static readonly TimeSpan ttl = TimeSpan.FromSeconds(0.2);

    [Theory]
    [InlineData(10, 10)]
    [InlineData(10, 20)]
    [InlineData(10, 50)]
    [InlineData(10, 100)]
    public async Task TaestAsync(int expectedIterations, int factoryDelayMls)
    {
        //  arange
        var waiting = ttl * expectedIterations;
        var calcTime = TimeSpan.FromMilliseconds(factoryDelayMls);
        var expected1 = waiting / (calcTime + ttl);
        var expected = (int)Math.Round(expected1);

        var runCount = 0;
        async Task<int> factory()
        {
            await Task.Delay(calcTime);
            return ++runCount;
        }

        var svc = new TtlBackground<int>(ttl, factory);
        await Task.Delay(waiting);

        //  test
        var actual = await svc.Value;

        //  assert
        Assert.Equal(expected, actual);
    }
}