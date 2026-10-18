using Simple.DI;

namespace Test.DI;

public class ResolverTests
{
    private const string Key = "test Key";
    private readonly Resolver<string> _svc = new Resolver<string>(null);
    private readonly Func<object> _factory = () => Key;


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
        _svc.Register(Key, _factory);

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
            _svc.RegisterScoped(Key, _factory);

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
            Assert.True(_svc.TryRegister(Key, _factory));
        }

        //  test
        var actual = _svc.TryRegister(Key, _factory);

        //  assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task MultithreadAsync()
    {
        //  arrange
        var keys = Enumerable.Range(0, 100).Select(i => $"Key {i}").ToList();

        //  test
        var tasks = keys.Select(i => Task.Run(() => _svc.Register(i, () => i)));
        await Task.WhenAll(tasks);

        //  assert
        keys.ForEach(i => TestAssert(i, i));
    }

    [Fact]
    public void Scopes_AreIsolatedBetweenContainers()
    {
        //  arrange
        var other = new Resolver<string>(null);

        using (var _1 = _svc.CreateScope())
        {
            //  arrange
            _svc.RegisterScoped(Key, _factory);

            //  test + assert: current container sees its scope
            Assert.NotEqual(_svc, _svc.ResolverScoped);
            Assert.Equal(Key, _svc.GetService(Key));

            //  assert: other container is unaffected (own scope stack)
            Assert.Equal(other, other.ResolverScoped);
            Assert.Null(other.GetService(Key));
        }
    }

    [Fact]
    public void TryRegister_Concurrent_SingleWinner()
    {
        //  arrange
        const int Attempts = 100;
        var winners = 0;

        //  test
        Parallel.For(0, Attempts, _ =>
        {
            if (_svc.TryRegister(Key, _factory))
            {
                Interlocked.Increment(ref winners);
            }
        });

        //  assert: exactly one registration wins
        Assert.Equal(1, winners);
        Assert.Equal(Key, _svc.GetService(Key));
    }

    [Fact]
    public void Dispose_OutOfOrder_UnwindsDescendants()
    {
        //  arrange
        var outer = _svc.CreateScope();
        var inner = _svc.CreateScope();

        //  test: out-of-order dispose of the ancestor
        outer.Dispose();

        //  assert: ancestor + descendant are both gone
        Assert.Equal(_svc, _svc.ResolverScoped);

        //  test: late dispose of the already-unwound descendant
        inner.Dispose();

        //  assert: no stale scope resurrected
        Assert.Equal(_svc, _svc.ResolverScoped);

        //  test + assert: fresh scope still works
        using var fresh = _svc.CreateScope();
        Assert.NotEqual(_svc, _svc.ResolverScoped);
    }

    [Fact]
    public void GetServices_PredicateFilters()
    {
        //  arrange
        const string Other = "other Key";
        _svc.Register(Key, _factory);
        _svc.Register(Other, () => Other);

        //  test
        var actual = _svc.GetServices(k => k == Key).ToArray();

        //  assert
        Assert.Single(actual);
        Assert.Equal(Key, actual[0]);
    }

    [Fact]
    public void Scope_DisposesScopedService()
    {
        //  arrange
        var disposed = false;

        using (var _1 = _svc.CreateScope())
        {
            //  register + create
            _svc.RegisterScoped(Key, () => new Tracked(() => disposed = true));
            _svc.GetService(Key);
        }

        //  assert
        Assert.True(disposed);
    }

    [Fact]
    public void ParentResolver_FallsBackToParentRegistration()
    {
        //  arrange: child over an explicit parent resolver (not a scope)
        _svc.Register(Key, _factory);
        var child = new Resolver<string>(_svc);

        //  test + assert: resolve walks the parent chain
        Assert.Equal(Key, child.GetService(Key));

        //  test + assert: child registration shadows the parent's one
        child.Register(Key, () => "child");
        Assert.Equal("child", child.GetService(Key));
        Assert.Equal(Key, _svc.GetService(Key));
    }

    [Fact]
    public void Scope_DoubleDispose_DisposesOnce()
    {
        //  arrange
        var count = 0;
        var scope = _svc.CreateScope();
        _svc.RegisterScoped(Key, () => new Tracked(() => count++));
        _svc.GetService(Key);

        //  test
        scope.Dispose();
        scope.Dispose();

        //  assert: the disposed guard skips the second pass
        Assert.Equal(1, count);
    }

    [Fact]
    public void Scope_DisposeAggregatesExceptions()
    {
        //  arrange
        const string Second = "second Key";
        var disposed = new List<string>();

        //  test: one service throws while disposing
        var ex = Assert.Throws<AggregateException>(() =>
        {
            using (var _1 = _svc.CreateScope())
            {
                _svc.RegisterScoped(Key, () => new Tracked(() => throw new InvalidOperationException("boom")));
                _svc.RegisterScoped(Second, () => new Tracked(() => disposed.Add(Second)));
                _svc.GetService(Key);
                _svc.GetService(Second);
            }
        });

        //  assert: every service still got its Dispose call (reverse order), errors are aggregated
        Assert.Single(ex.InnerExceptions);
        Assert.Equal(new[] { Second }, disposed);
    }

    [Fact]
    public void ScopeProvider_PushNull_Throws()
    {
        //  arrange
        var provider = new ScopeProvider<string>();

        //  test + assert
        Assert.Throws<ArgumentNullException>(() => provider.Push(null!));
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

    private sealed class Tracked(Action onDispose) : IDisposable
    {
        public void Dispose() => onDispose();
    }
}