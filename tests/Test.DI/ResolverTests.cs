using Simple.DI;

namespace Test.DI;

public class ResolverTests
{
    private const string Key = "test Key";
    private readonly Resolver<string> _svc = new Resolver<string>(null);
    private readonly Func<object> _facttory = () => Key;


    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void RegisterOverride(uint regCount)
    {
        //  arrange
        while (regCount-- > 0)
        {
            _svc.Register(Key, () => $"{Key} {regCount}");
        }
        _svc.Register(Key, _facttory);

        //  test
        TestAssert(Key, Key);
    }

    [Fact]
    public void RegisterScoped()
    {
        const string Key2 = "test Key 2";

        using (var _1 = _svc.CreateScope())
        {
            //  arrange 1
            _svc.RegisterScoped(Key, _facttory);

            //  test 1
            TestAssert(Key, Key, true);

            //  test 2
            TestAssert(null, Key2, true);

            using (var _2 = _svc.CreateScope())
            {
                //  arrange 2
                _svc.RegisterScoped(Key2, () => Key2);

                //  test 2
                TestAssert(Key2, Key2, true);

                //  test 1
                TestAssert(Key, Key, true);
            }

            //  test 1
            TestAssert(Key, Key, true);

            //  test 2
            TestAssert(null, Key2, true);
        }

        //  test
        TestAssert(null, Key);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TryRegister(bool expected)
    {
        //  assert
        if (!expected)
        {
            Assert.True(_svc.TryRegister(Key, _facttory));
        }

        //  test
        var actual = _svc.TryRegister(Key, _facttory);

        //  assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task MultithteadAsync()
    {
        //  arrange
        var keys = Enumerable.Range(0, 100).Select(i => $"Key {i}").ToList();

        //  test
        var tasks = keys.Select(i => Task.Run(() => _svc.Register(i, () => i)));
        await Task.WhenAll(tasks);

        //  assert
        keys.ForEach(i => TestAssert(i, i));
    }


    private void TestAssert(object? expected, string key, bool isScoped = false)
    {
        //  test
        var actual = _svc.GetService(key);

        //  assert
        Assert.Equal(expected, actual);
        if (isScoped)
        {
            Assert.NotEqual(_svc, _svc.ResolverScoped);
        }
        else
        {
            Assert.Equal(_svc, _svc.ResolverScoped);
        }
    }
}