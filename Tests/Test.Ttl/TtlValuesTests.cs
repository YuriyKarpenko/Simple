using Simple.Ttl;

namespace Test.Ttl;

public class TtlValuesTests
{
    private static readonly TimeSpan ttl = TimeSpan.FromMilliseconds(100);

    [Fact]
    public void GetOrCreate_CreatesOnce_AndCaches()
    {
        //  arrange
        var svc = new TtlValues(ttl);
        var calls = 0;
        int factory() => ++calls;

        //  test
        var first = svc.GetOrCreate(factory);
        var second = svc.GetOrCreate(factory);

        //  assert
        Assert.Equal(1, first);
        Assert.Equal(1, second);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void GetOrCreate_DifferentTypes_KeptSeparately()
    {
        //  arrange
        var svc = new TtlValues(ttl);

        //  test
        var number = svc.GetOrCreate(() => 42);
        var text = svc.GetOrCreate(() => "ttl");

        //  assert: one dictionary entry per typeof(T)
        Assert.Equal(42, number);
        Assert.Equal("ttl", text);
    }

    [Fact]
    public void GetOrCreate_Expired_Recreates()
    {
        //  arrange
        var svc = new TtlValues(ttl);
        var calls = 0;
        int factory() => ++calls;

        //  test
        var first = svc.GetOrCreate(factory);
        Thread.Sleep(ttl + ttl);
        var second = svc.GetOrCreate(factory);

        //  assert
        Assert.Equal(1, first);
        Assert.Equal(2, second);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task GetOrCreateAsync_CreatesOnce_AndCaches()
    {
        //  arrange
        var svc = new TtlValues(ttl);
        var calls = 0;
        async Task<int> factory()
        {
            await Task.Yield();
            return ++calls;
        }

        //  test
        var first = await svc.GetOrCreateAsync(factory);
        var second = await svc.GetOrCreateAsync(factory);

        //  assert
        Assert.Equal(1, first);
        Assert.Equal(1, second);
        Assert.Equal(1, calls);
    }
}
