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
        Func<object> factory = () => new Custom1();
        var t = typeof(Custom1);
        if (!expected)
        {
            Assert.True(_svc.TryRegister(t, factory));
        }

        //  test
        var actual = _svc.TryRegister(t, factory);

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

    [Fact]
    public void AddTransient_InsideScope_SurvivesAfterDispose()
    {
        using (var _1 = _svc.CreateScope())
        {
            //  arrange
            _svc.AddTransient(typeof(Custom1));
        }

        //  test
        var a = _svc.GetService(typeof(Custom1));
        var b = _svc.GetService(typeof(Custom1));

        //  assert: resolvable after scope dispose, new instance per resolve (transient)
        Assert.IsType<Custom1>(a);
        Assert.IsType<Custom1>(b);
        Assert.NotSame(a, b);
    }

    [Fact]
    public void AddScoped_Factory_NewInstancePerResolve()
    {
        using (var _1 = _svc.CreateScope())
        {
            //  arrange
            _svc.AddScoped(_ => new Custom1());

            //  test
            var a = _svc.GetService(typeof(Custom1));
            var b = _svc.GetService(typeof(Custom1));

            //  assert: transient — factory runs per resolve, registration lives in the scope
            Assert.IsType<Custom1>(a);
            Assert.NotSame(a, b);
        }

        //  assert
        Assert.Null(_svc.GetService(typeof(Custom1)));
    }

    [Fact]
    public void AddScopedSingleton_CachesWithinScope()
    {
        //  arrange
        var calls = 0;

        using (var _1 = _svc.CreateScope())
        {
            //  arrange
            _svc.AddScopedSingleton(_ =>
            {
                calls++;
                return new Custom1();
            });

            //  test
            var a = _svc.GetService(typeof(Custom1));
            var b = _svc.GetService(typeof(Custom1));

            //  assert: one instance per registration (this scope)
            Assert.Same(a, b);
            Assert.Equal(1, calls);
        }

        //  assert: registration dies with the scope
        Assert.Null(_svc.GetService(typeof(Custom1)));
    }

    [Fact]
    public void AddScopedSingleton_RegisteredOutsideScope_CachesInRoot()
    {
        //  arrange
        _svc.AddScopedSingleton(() => new Custom1());

        //  test
        var a = _svc.GetService(typeof(Custom1));
        var b = _svc.GetService(typeof(Custom1));

        //  assert: registration outside any scope lives in root — one instance
        Assert.Same(a, b);
    }

    [Fact]
    public void AddSingleton_Concurrent_SingleInstance()
    {
        //  arrange
        var svc = new Resolver();
        var calls = 0;
        svc.AddSingleton(() =>
        {
            Interlocked.Increment(ref calls);
            return new object();
        });

        //  test
        var instances = new object[20];
        Parallel.For(0, instances.Length, i => instances[i] = svc.GetService<object>()!);

        //  assert: single factory call, single instance (was: instance ??= race)
        Assert.Equal(1, calls);
        Assert.All(instances, x => Assert.Same(instances[0], x));
    }

    [Fact]
    public void Scope_DisposesScopedServices_InReverseOrder()
    {
        //  arrange
        var log = new List<string>();

        using (var _1 = _svc.CreateScope())
        {
            //  register + create two scoped services
            _svc.AddScoped(_ => new TrackerA(log));
            _svc.AddScoped(_ => new TrackerB(log));
            _svc.GetService(typeof(TrackerA));
            _svc.GetService(typeof(TrackerB));
        }

        //  assert: reverse creation order
        Assert.Equal(new[] { "b", "a" }, log);
    }

    [Fact]
    public void Scope_DisposesEveryTransientInstance()
    {
        //  arrange
        var log = new List<string>();

        using (var _1 = _svc.CreateScope())
        {
            //  register transient scoped service + create two instances
            _svc.AddScoped(_ => new TrackerA(log));
            _svc.GetService(typeof(TrackerA));
            _svc.GetService(typeof(TrackerA));
        }

        //  assert: both created instances disposed
        Assert.Equal(2, log.Count);
    }

    [Fact]
    public void Scope_DisposesScopedSingletonOnce()
    {
        //  arrange
        var log = new List<string>();

        using (var _1 = _svc.CreateScope())
        {
            //  register scoped singleton + resolve twice
            _svc.AddScopedSingleton(_ => new TrackerA(log));
            _svc.GetService(typeof(TrackerA));
            _svc.GetService(typeof(TrackerA));
        }

        //  assert: single instance — disposed once
        Assert.Equal(new[] { "a" }, log);
    }

    [Fact]
    public void Scope_DoesNotDisposeRootRegistrations()
    {
        //  arrange
        var log = new List<string>();
        _svc.AddSingleton(() => new TrackerA(log));

        using (var _1 = _svc.CreateScope())
        {
            _svc.GetService(typeof(TrackerA));
        }

        //  assert: contract — only registrations made inside the scope are disposed
        Assert.Empty(log);
    }

    [Fact]
    public void AddScopedSingleton_ConcurrentFirstResolve_SingleInstance()
    {
        //  arrange
        var calls = 0;

        using (var _1 = _svc.CreateScope())
        {
            //  arrange
            _svc.AddScopedSingleton(() =>
            {
                Interlocked.Increment(ref calls);
                return new Custom1();
            });

            //  test
            var instances = new Custom1[20];
            Parallel.For(0, instances.Length, i => instances[i] = _svc.GetService<Custom1>()!);

            //  assert: single factory call under contention (memoize double-checked lock)
            Assert.Equal(1, calls);
            Assert.All(instances, x => Assert.Same(instances[0], x));
        }
    }

    [Fact]
    public void ScopedFactory_ResolvesFromSameScope()
    {
        //  arrange
        var log = new List<string>();

        using (var _1 = _svc.CreateScope())
        {
            _svc.AddScoped(_ => new TrackerA(log));
            _svc.AddScoped(sp => new TrackerB(log, (TrackerA)sp.GetService(typeof(TrackerA))!));

            //  test: nested resolve inside a scoped factory sees the same scope
            var b = (TrackerB)_svc.GetService(typeof(TrackerB))!;

            //  assert
            Assert.NotNull(b.Dep);
        }

        //  assert: a created first, b second — disposed in reverse order
        Assert.Equal(new[] { "b", "a" }, log);
    }

    [Fact]
    public void GetServices_ExtensionEnumeratesOnResolver()
    {
        //  arrange
        _svc.AddSingleton(() => new Custom1());

        //  test
        var actual = _svc.GetServices<ICustom>().ToArray();

        //  assert
        Assert.Single(actual);
        Assert.IsType<Custom1>(actual[0]);
    }

    [Fact]
    public void GetServices_ExtensionOnNonResolverProvider_Throws()
    {
        //  arrange
        IServiceProvider foreign = new ForeignProvider();

        //  test + assert: was a silent empty result
        Assert.Throws<InvalidOperationException>(() => foreign.GetServices<ICustom>());
        Assert.Throws<InvalidOperationException>(() => ((IServiceProvider)null!).GetServices<ICustom>());
    }

    [Fact]
    public void GetService_ExtensionOnNullProvider_ReturnsNull()
    {
        //  test + assert
        Assert.Null(((IServiceProvider)null!).GetService<Custom1>());
    }

    [Fact]
    public void OpenGenericRegistration_ResolvesByClosedKey()
    {
        //  arrange: registration key is the open type typeof(List<>)
        _svc.Register(typeof(List<>), _ => new List<int> { 1, 2 });

        //  test: closed generic key maps to its definition
        var actual = _svc.GetService(typeof(List<int>));

        //  assert
        Assert.IsType<List<int>>(actual);
    }

    [Fact]
    public void ClosedGenericRegistration_ResolvesByExactKey()
    {
        //  arrange: registration key is the closed type typeof(List<int>)
        _svc.AddSingleton(() => new List<int> { 1 });

        //  test + assert: closed registration must resolve directly
        Assert.IsType<List<int>>(_svc.GetService(typeof(List<int>)));
    }

    [Fact]
    public void ClosedGeneric_NullResult_FallsBackToOpenRegistration()
    {
        //  arrange: closed factory returns null
        _svc.AddSingleton<List<int>>(() => null!);
        _svc.Register(typeof(List<>), _ => new List<int> { 1 });

        //  test + assert: null from the closed factory falls through to the definition registration
        Assert.IsType<List<int>>(_svc.GetService(typeof(List<int>)));
    }

    [Fact]
    public void RegisterScoped_OutsideScope_RegistersInRoot()
    {
        //  arrange: no active scope — registration lands in the root
        var log = new List<string>();
        _svc.RegisterScoped(typeof(TrackerA), _ => new TrackerA(log));

        using (var _1 = _svc.CreateScope())
        {
            //  test: resolvable through a scope
            Assert.IsType<TrackerA>(_svc.GetService(typeof(TrackerA)));
        }

        //  assert: root registration outlives the scope and is never disposed
        Assert.IsType<TrackerA>(_svc.GetService(typeof(TrackerA)));
        Assert.Empty(log);
    }

    [Fact]
    public void AddScoped_OutsideScope_RegistersInRoot()
    {
        //  arrange: registration without a scope lands in the root
        _svc.AddScoped(() => new Custom1());

        //  test
        var a = _svc.GetService(typeof(Custom1));
        var b = _svc.GetService(typeof(Custom1));

        //  assert: root registration survives scopes, transient per resolve
        Assert.IsType<Custom1>(a);
        Assert.NotSame(a, b);
        using (var _1 = _svc.CreateScope())
        {
            Assert.NotNull(_svc.GetService(typeof(Custom1)));
        }
    }

    [Fact]
    public void AddTransient_ValueType_ResolvesDefault()
    {
        //  arrange: value types skip expression compilation (Activator fallback path)
        _svc.AddTransient(typeof(int));

        //  test + assert
        Assert.Equal(0, _svc.GetService(typeof(int)));
    }

    [Fact]
    public void AddTransient_Interface_ThrowsAtResolve()
    {
        //  arrange: no expression can be compiled for an interface — Activator fallback
        _svc.AddTransient(typeof(ICustom));

        //  test + assert: same error timing as before (at resolve, not registration)
        Assert.Throws<MissingMethodException>(() => _svc.GetService(typeof(ICustom)));
    }

    [Fact]
    public void AddSingleton_NullResult_CachedOnce()
    {
        //  arrange
        var calls = 0;
        _svc.AddSingleton<Custom1>(() =>
        {
            calls++;
            return null!;
        });

        //  test
        var a = _svc.GetService(typeof(Custom1));
        var b = _svc.GetService(typeof(Custom1));

        //  assert: null result is cached too (was: instance ??= re-invoked on every resolve)
        Assert.Null(a);
        Assert.Null(b);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void UsingSetup_NullArguments_Throw()
    {
        //  test + assert
        Assert.Throws<ArgumentNullException>(() => DiExtensions.UsingSetup(null!, _ => { }));
        Assert.Throws<ArgumentNullException>(() => DiExtensions.UsingSetup(new Resolver(), null!));
    }

    [Fact]
    public void BuildServiceProvider_ReturnsSelf()
    {
        //  test + assert
        Assert.Same(_svc, _svc.BuildServiceProvider());
    }

    [Fact]
    public void SingletonTypeOverloads_Resolve()
    {
        //  arrange
        var svc = new Resolver();
        svc.AddSingleton(typeof(Custom1));
        svc.AddSingleton<ISecond>(typeof(Second1));
        svc.AddSingleton<ICustom, Custom2>();

        //  test + assert: one instance per registration
        Assert.Same(svc.GetService(typeof(Custom1)), svc.GetService(typeof(Custom1)));
        Assert.IsType<Second1>(svc.GetService(typeof(ISecond)));
        Assert.Same(svc.GetService(typeof(ICustom)), svc.GetService(typeof(ICustom)));
    }

    [Fact]
    public void TransientTypeOverloads_Resolve()
    {
        //  arrange
        var svc = new Resolver();
        svc.AddTransient(typeof(Custom1));
        svc.AddTransient<ISecond>(typeof(Second1));
        svc.AddTransient<ICustom, Custom2>();

        //  test
        var a = svc.GetService(typeof(Custom1));
        var b = svc.GetService(typeof(Custom1));

        //  assert: new instance per resolve
        Assert.IsType<Custom1>(a);
        Assert.NotSame(a, b);
        Assert.IsType<Second1>(svc.GetService(typeof(ISecond)));
        Assert.IsType<Custom2>(svc.GetService(typeof(ICustom)));
    }

    [Fact]
    public void ScopedTypeOverloads_Resolve()
    {
        //  arrange
        var svc = new Resolver();

        using (var _1 = svc.CreateScope())
        {
            svc.AddScoped(typeof(Custom1));
            svc.AddScoped<ISecond>(typeof(Second1));
            svc.AddScoped<ICustom, Custom2>();

            //  test
            var a = svc.GetService(typeof(Custom1));
            var b = svc.GetService(typeof(Custom1));

            //  assert: factory runs per resolve, registration dies with the scope
            Assert.IsType<Custom1>(a);
            Assert.NotSame(a, b);
            Assert.IsType<Second1>(svc.GetService(typeof(ISecond)));
            Assert.IsType<Custom2>(svc.GetService(typeof(ICustom)));
        }

        //  assert
        Assert.Null(svc.GetService(typeof(Custom1)));
    }

    [Fact]
    public void ScopedSingletonTypeOverloads_Resolve()
    {
        //  arrange
        var svc = new Resolver();

        using (var _1 = svc.CreateScope())
        {
            svc.AddScopedSingleton(typeof(Custom1));
            svc.AddScopedSingleton<ISecond>(typeof(Second1));
            svc.AddScopedSingleton<ICustom, Custom2>();

            //  test + assert: one instance per registration
            Assert.Same(svc.GetService(typeof(Custom1)), svc.GetService(typeof(Custom1)));
            Assert.Same(svc.GetService(typeof(ISecond)), svc.GetService(typeof(ISecond)));
            Assert.Same(svc.GetService(typeof(ICustom)), svc.GetService(typeof(ICustom)));
        }

        //  assert
        Assert.Null(svc.GetService(typeof(Custom1)));
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
    public interface ISecond { }
    private class Custom1 : ICustom { }
    private class Custom2 : Custom1 { }
    private class Custom3 : Custom2 { }
    private class Second1 : ISecond { }

    private sealed class TrackerA(List<string> log) : IDisposable
    {
        public void Dispose() => log.Add("a");
    }

    private sealed class TrackerB(List<string> log, TrackerA? dep = null) : IDisposable
    {
        public TrackerA? Dep { get; } = dep;
        public void Dispose() => log.Add("b");
    }

    private sealed class ForeignProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}