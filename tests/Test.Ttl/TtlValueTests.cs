using Simple.Ttl;

namespace Test.Ttl
{
    public class TtlValueTests
    {
        private static readonly TimeSpan      _ttl    = TimeSpan.FromSeconds(0.2);
        private static readonly TimeSpan      _margin = TimeSpan.FromMilliseconds(50); //  safety over the ttl boundary
        private        readonly TtlValue<int> _svc    = new TtlValue<int>(_ttl);

        [Fact]
        public void GetOrCreate()
        {
            //  arange
            var runCount = 0;
            var factory  = () => ++runCount;

            //  test
            var actual0 = _svc.GetOrCreate(factory);
            var actual1 = _svc.GetOrCreate(factory);
            Thread.Sleep(_ttl + _margin);
            var actual2 = _svc.GetOrCreate(factory);

            //  assert
            Assert.Equal(1, actual0);
            Assert.Equal(1, actual1);
            Assert.Equal(2, actual2);
            Assert.Equal(2, runCount);
        }

        [Fact]
        public async Task GetOrCreateAsync()
        {
            //  arange
            var runCount = 0;
            var factory  = () => Task.FromResult(++runCount);

            //  test
            var actual0 = await _svc.GetOrCreateAsync(factory);
            var actual1 = await _svc.GetOrCreateAsync(factory);
            await Task.Delay(_ttl + _ttl); //  wide margin: the expiry bump lands on a fire-and-forget continuation
            var actual2 = await _svc.GetOrCreateAsync(factory);

            //  assert
            Assert.Equal(1, actual0);
            Assert.Equal(1, actual1);
            Assert.Equal(2, actual2);
            Assert.Equal(2, runCount);
        }

        [Fact]
        public async Task GetOrCreateAsync_ThrowingFactory_PropagatesException()
        {
            //  arange
            var runCount = 0;

            async Task<int> factory()
            {
                await Task.Yield();
                if (++runCount == 1)
                {
                    throw new InvalidOperationException("boom");
                }
                return runCount;
            }

            //  test
            var ex     = await Assert.ThrowsAsync<InvalidOperationException>(() => _svc.GetOrCreateAsync(factory));
            var actual = await _svc.GetOrCreateAsync(factory);

            //  assert
            Assert.Equal("boom", ex.Message);
            Assert.Equal(2, actual);
            Assert.Equal(2, runCount);
        }

        [Theory]
        [InlineData(false, 10, 1)]
        [InlineData(false, 20, 1)]
        [InlineData(true, 10, 1)]
        [InlineData(true, 200, 1)]
        public async Task GetOrCreateAsync_Parallel(bool isRunFirs, int iterations, int expected)
        {
            //  arange
            var runCount = 0;

            async Task<int> factory()
            {
                await Task.Delay(100);
                return ++runCount;
            }

            var actions = Enumerable
                .Range(0, iterations)
                .Select<int, Action>(_ => async () => await _svc.GetOrCreateAsync(factory))
                .ToArray();

            //  test
            if (isRunFirs)
            {
                //  warm-up: the class is already initialized
                _ = await _svc.GetOrCreateAsync(factory);
            }
            Parallel.Invoke(actions);
            var actual = await _svc.GetOrCreateAsync(factory);

            //  assert
            Assert.Equal(expected, actual);
            Assert.Equal(expected, runCount);
        }

        [Theory]
        [InlineData(6, 2, 3)]
        public void MultithreadCount(int iterations, int parts, int expected)
        {
            //  arrange
            var runCount = 0;
            var factory  = () => ++runCount;
            var actuals  = new List<int>(iterations);
            var partTtl  = _ttl / parts;

            //  test
            while (iterations-- > 0)
            {
                actuals.Add(_svc.GetOrCreate(factory));
                Thread.Sleep(partTtl);
            }

            //  assert
            var d = actuals.GroupBy(x => x).Select(i => $"{i.Key} -> {i.Count()}");
            Assert.Equal(expected, d.Count());
            Assert.Equal(expected, runCount);
        }

        [Theory]
        [InlineData(6, 2, 3)]
        public async Task MultithreadCountAsync(int iterations, int parts, int expected)
        {
            //  arrange
            var runCount = 0;
            var factory  = () => Task.FromResult(++runCount);
            var actuals  = new List<int>(iterations);
            var partTtl  = _ttl / parts;

            //  test
            while (iterations-- > 0)
            {
                actuals.Add(await _svc.GetOrCreateAsync(factory));
                await Task.Delay(partTtl);
            }

            //  assert
            var d = actuals.GroupBy(x => x).Select(i => $"{i.Key} -> {i.Count()}");
            Assert.Equal(expected, d.Count());
            Assert.Equal(expected, runCount);
        }

        [Fact]
        public async Task MultithreadDuration()
        {
            //  arrange: hammer the class for half a ttl window and stop well before the boundary -
            //  a call issued on the boundary would legitimately re-create the value, which would
            //  make "exactly one factory run" depend on the scheduler
            var runCount = 0;
            var factory  = () => Task.FromResult(++runCount);
            var expired  = DateTime.UtcNow + (_ttl / 2);

            //  test
            while (expired > DateTime.UtcNow)
            {
                await _svc.GetOrCreateAsync(factory);
            }

            //  assert
            Assert.Equal(1, runCount);
        }

        [Fact]
        public void Set()
        {
            //  arange
            const int value = 14;

            //  test
            _svc.Set(value);
            var actual1 = _svc.TryGetValue(out var v1);
            Thread.Sleep(_ttl + _margin);
            var actual2 = _svc.IsExpired;

            //  assert
            Assert.Equal(value, v1);
            Assert.True(actual1);
            Assert.True(actual2);
        }

        [Fact]
        public void TryGetValue()
        {
            //  arange
            const int value = 14;
            _svc.Set(value);

            //  test
            var actual1 = _svc.TryGetValue(out var v1);
            Thread.Sleep(_ttl + _margin);
            var actual2 = _svc.TryGetValue(out var v2);

            //  assert
            Assert.Equal(value, v1);
            Assert.True(actual1);
            Assert.False(actual2);
        }

        [Fact]
        public void GetOrCreate_StaleWhileRevalidate_ServesStaleAndRefreshesInBackground()
        {
            //  arrange
            var svc      = new TtlValue<int>(_ttl, true);
            var runCount = 0;
            var factory  = () => ++runCount;

            //  test
            var actual0 = svc.GetOrCreate(factory);
            svc.GetOrCreate(factory);
            Thread.Sleep(_ttl);
            var stale = svc.GetOrCreate(factory);

            //  assert
            Assert.Equal(1, actual0);
            Assert.Equal(1, stale);
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);
            var actual   = stale;
            while (actual != 2 && DateTime.UtcNow < deadline)
            {
                actual = svc.GetOrCreate(factory);
                Thread.Sleep(10);
            }
            Assert.Equal(2, actual);
            Assert.Equal(2, runCount);
        }

        [Fact]
        public void GetOrCreate_WithoutStaleWhileRevalidate_WaitsForFresh()
        {
            //  arrange
            var runCount = 0;
            var factory  = () => ++runCount;

            //  test: class-level svc created without the option
            _svc.GetOrCreate(factory);
            Thread.Sleep(_ttl + _margin);
            var actual = _svc.GetOrCreate(factory);

            //  assert: old behavior — caller waits for the fresh value
            Assert.Equal(2, actual);
            Assert.Equal(2, runCount);
        }

        [Fact]
        public async Task GetOrCreateAsync_StaleWhileRevalidate_ServesStaleAndRefreshesInBackground()
        {
            //  arrange
            var svc      = new TtlValue<int>(_ttl, true);
            var runCount = 0;
            var factory  = () => Task.FromResult(++runCount);

            //  test
            var actual0 = await svc.GetOrCreateAsync(factory);
            await svc.GetOrCreateAsync(factory);
            await Task.Delay(_ttl);
            var stale = await svc.GetOrCreateAsync(factory);

            //  assert
            Assert.Equal(1, actual0);
            Assert.Equal(1, stale);
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);
            var actual   = stale;
            while (actual != 2 && DateTime.UtcNow < deadline)
            {
                actual = await svc.GetOrCreateAsync(factory);
                await Task.Delay(10);
            }
            Assert.Equal(2, actual);
            Assert.Equal(2, runCount);
        }

        [Fact]
        public void GetOrCreate_StaleWhileRevalidate_ConcurrentCalls_StartOneRefresh()
        {
            //  arrange
            var swrTtl   = TimeSpan.FromMilliseconds(300);
            var svc      = new TtlValue<int>(swrTtl, true);
            var runCount = 0;
            var factory = () =>
            {
                Thread.Sleep(100);
                return ++runCount;
            };

            //  test
            var initial = svc.GetOrCreate(factory);
            Thread.Sleep(swrTtl);
            var results = new int[10];
            Parallel.For(0, 10, i => results[i] = svc.GetOrCreate(factory));

            //  assert: every concurrent caller gets the stale value
            Assert.Equal(1, initial);
            Assert.All(results, r => Assert.Equal(1, r));
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(3);
            var actual   = 1;
            while (actual != 2 && DateTime.UtcNow < deadline)
            {
                actual = svc.GetOrCreate(factory);
                Thread.Sleep(10);
            }
            Assert.Equal(2, actual);
            Assert.Equal(2, runCount);
        }

        [Fact]
        public void GetOrCreate_ThrowingFactory_RetriesOnNextCall()
        {
            //  arrange: expiry is bumped only on success — a thrown factory must not poison the ttl window
            var runCount = 0;

            int factory()
            {
                runCount++;
                if (runCount == 1)
                {
                    throw new InvalidOperationException();
                }
                return runCount;
            }

            //  test
            Assert.Throws<InvalidOperationException>(() => _svc.GetOrCreate(factory));
            var actual = _svc.GetOrCreate(factory);

            //  assert
            Assert.Equal(2, actual);
            Assert.Equal(2, runCount);
        }

        [Fact]
        public void TryGetValue_BeforeFirstValue_ReturnsFalse()
        {
            //  test
            var actual = _svc.TryGetValue(out int _);

            //  assert
            Assert.False(actual);
            Assert.True(_svc.IsExpired);
        }

        [Fact]
        public async Task GetOrCreateAsync_StaleWhileRevalidate_FaultedRefresh_KeepsStaleAndRetries()
        {
            //  arrange
            var svc      = new TtlValue<int>(_ttl, true);
            var runCount = 0;

            async Task<int> factory()
            {
                await Task.Yield();
                if (++runCount == 2)
                {
                    throw new InvalidOperationException("refresh failed");
                }
                return runCount;
            }

            //  test
            var initial = await svc.GetOrCreateAsync(factory);
            await Task.Delay(_ttl);
            var stale    = await svc.GetOrCreateAsync(factory);
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);
            var actual   = stale;
            while (actual != 3 && DateTime.UtcNow < deadline)
            {
                await Task.Delay(10);
                actual = await svc.GetOrCreateAsync(factory);
            }

            //  assert
            Assert.Equal(1, initial);
            Assert.Equal(1, stale);
            Assert.Equal(3, actual);
            Assert.Equal(3, runCount);
        }

        [Fact]
        public void GetOrCreate_StaleWhileRevalidate_FaultedRefresh_KeepsStaleAndRetries()
        {
            //  arrange
            var svc      = new TtlValue<int>(_ttl, true);
            var runCount = 0;

            int factory()
            {
                if (++runCount == 2)
                {
                    throw new InvalidOperationException("refresh failed");
                }
                return runCount;
            }

            //  test
            var initial = svc.GetOrCreate(factory);
            Thread.Sleep(_ttl);
            var stale    = svc.GetOrCreate(factory);
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);
            var actual   = stale;
            while (actual != 3 && DateTime.UtcNow < deadline)
            {
                actual = svc.GetOrCreate(factory);
                Thread.Sleep(10);
            }

            //  assert: a faulted background refresh keeps the stale value, the next call retries
            Assert.Equal(1, initial);
            Assert.Equal(1, stale);
            Assert.Equal(3, actual);
            Assert.Equal(3, runCount);
        }
    }
}