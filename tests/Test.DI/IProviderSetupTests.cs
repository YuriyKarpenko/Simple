using Simple.DI;

namespace Test.DI;

public class IProviderSetupTests
{
    private readonly IProviderSetup _svc = new Resolver(null);
    private readonly Custom1 c1 = new Custom1();
    private readonly Custom2 c2 = new Custom2();
    private readonly Custom3 c3 = new Custom3();

    [Fact]
    public void RegisterOverride()
    {
        //  arrange
        _svc.AddSingleton<ICustom>(c1);
        _svc.AddSingleton<ICustom>(c2);
        _svc.AddSingleton<ICustom>(c3);

        //  test
        TestAssert<ICustom>(c3);
    }

    [Fact]
    public void RegisterScoped()
    {
        using (var _1 = _svc.CreateScope())
        {
            //  arrange 1
            _svc.AddScoped(() => c1);

            //  test 1
            TestAssert<Custom1>(c1, true);

            //  test 2
            TestAssert<Custom2>(null, true);

            using (var _2 = _svc.CreateScope())
            {
                //  arrange 2
                _svc.AddScoped(() => c2);

                //  test 2
                TestAssert<Custom2>(c2, true);

                //  test 1
                TestAssert<Custom1>(c1, true);
            }

            //  test 1
            TestAssert<Custom1>(c1, true);

            //  test 2
            TestAssert<Custom2>(null, true);
        }

        //  test
        TestAssert<Custom1>(null);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TryRegister(bool expected)
    {
        //  assert
        Func<object> facttory = () => new Custom1();
        var t = typeof(Custom1);
        if (!expected)
        {
            Assert.True(_svc.TryRegister(t, facttory));
        }

        //  test
        var actual = _svc.TryRegister(t, facttory);

        //  assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetServices()
    {
        //  arrange
        var svc = new Resolver();
        svc.AddSingleton<Custom1>();
        svc.AddSingleton<Custom2>();
        using (var _1 = svc.CreateScope())
        {
            svc.AddScoped<Custom2>();
            svc.AddScoped(_ => new Custom3());

            Test<ICustom>(svc, 4);
            Test<Custom1>(svc, 4);
            Test<Custom2>(svc, 3);
            Test<Custom3>(svc, 1);
        }

        Test<ICustom>(svc, 2);
        Test<Custom1>(svc, 2);
        Test<Custom2>(svc, 1);
        Test<Custom3>(svc, 0);

        static void Test<T>(Resolver r, int expected)
        {
            //  test
            var actual = r.GetServices(typeof(T)).ToArray();

            //  assert
            Assert.Equal(expected, actual.Length);
        }
    }


    private void TestAssert<TKey>(object? expected, bool isScoped = false)
    {
        //  test
        var actual = _svc.GetService(typeof(TKey));

        //  assert
        Assert.Equal(expected, actual);
        if (_svc is Resolver svc)
        {
            if (isScoped)
            {
                Assert.NotEqual(_svc, svc.ScopedSp);
            }
            else
            {
                Assert.Equal(_svc, svc.ScopedSp);
            }
        }
    }

    public interface ICustom { }
    private class Custom1 : ICustom { }
    private class Custom2 : Custom1 { }
    private class Custom3 : Custom2 { }
}