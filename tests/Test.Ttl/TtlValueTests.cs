using Simple.Ttl;

namespace Test.Ttl
{
    public class TtlValueTests
    {
        private static readonly TimeSpan ttl = TimeSpan.FromSeconds(0.2);
        private readonly TtlValue<int> svc = new TtlValue<int>(ttl);

        [Fact]
        public void GetOrCreate()
        {
            //  arange
            var runCount = 0;
            var factory = () => ++runCount;

            //  test
            var actual0 = svc.GetOrCreate(factory);
            var actual1 = svc.GetOrCreate(factory);
            Thread.Sleep(ttl);
            var actual2 = svc.GetOrCreate(factory);

            //  assert
            Assert.Equal(1, actual0);
            Assert.Equal(1, actual1);
            Assert.Equal(2, actual2);
            Assert.Equal(2, runCount);
        }

        [Fact]
        public async void GetOrCreateAsync()
        {
            //  arange
            var runCount = 0;
            var factory = () => Task.FromResult(++runCount);

            //  test
            var actual0 = await svc.GetOrCreateAsync(factory);
            var actual1 = await svc.GetOrCreateAsync(factory);
            await Task.Delay(ttl);
            var actual2 = await svc.GetOrCreateAsync(factory);

            //  assert
            Assert.Equal(1, actual0);
            Assert.Equal(1, actual1);
            Assert.Equal(2, actual2);
            Assert.Equal(2, runCount);
        }

        [Fact]
        public async void GetOrCreateAsync_Parallel()
        {
            //  arange
            var runCount = 0;
            //var factory = () => Task.FromResult(++runCount);
            async Task<int> factory()
            {
                await Task.Delay(100);
                return ++runCount;
            }

            //  test
            Parallel.Invoke(
                async () => await svc.GetOrCreateAsync(factory),
                async () => await svc.GetOrCreateAsync(factory),
                async () => await svc.GetOrCreateAsync(factory),
                async () => await svc.GetOrCreateAsync(factory),
                async () => await svc.GetOrCreateAsync(factory),
                async () => await svc.GetOrCreateAsync(factory)
                );
            var actual = await svc.GetOrCreateAsync(factory);

            //  assert
            Assert.Equal(1, actual);
            Assert.Equal(1, runCount);
        }

        [Theory]
        [InlineData(10, 2, 5)]
        //[InlineData(100, 10, 10)]     //  +1 - corretion with time of executing test (ttl = 0:00:01.0)
        //[InlineData(100, 10, 13)]     //  +1 - corretion with time of executing test (ttl = 0:00:00.5)
        //[InlineData(100, 10, 15)]     //  +1 - corretion with time of executing test (ttl = 0:00:00.2)
        public void MultithteadCount(int iterations, int parts, int expected)
        {
            //  arrange
            var runCount = 0;
            var factory = () => ++runCount;
            var actuals = new List<int>(iterations);
            var partTtl = ttl / parts;

            //  test
            while (iterations-- > 0)
            {
                actuals.Add(svc.GetOrCreate(factory));
                Thread.Sleep(partTtl);
            }

            //  assert
            var d = actuals.GroupBy(x => x).Select(i => $"{i.Key} -> {i.Count()}");
            //Trace.TraceError(string.Join('\n', d));
            Assert.Equal(expected, d.Count());
            Assert.Equal(expected, runCount);
        }

        [Theory]
        [InlineData(10, 2, 5)]
        //[InlineData(100, 10, 15)]     //  +1 - corretion with time of executing test
        public async Task MultithteadCountAsync(int iterations, int parts, int expected)
        {
            //  arrange
            var runCount = 0;
            var factory = () => Task.FromResult(++runCount);
            var actuals = new List<int>(iterations);
            var partTtl = ttl / parts;

            //  test
            while (iterations-- > 0)
            {
                actuals.Add(await svc.GetOrCreateAsync(factory));
                await Task.Delay(partTtl);
            }

            //  assert
            var d = actuals.GroupBy(x => x).Select(i => $"{i.Key} -> {i.Count()}");
            //Trace.TraceError(string.Join('\n', d));
            Assert.Equal(expected, d.Count());
            Assert.Equal(expected, runCount);
        }

        [Fact]  //  TODO: Test is not stable
        public async Task MultithteadDuration()
        {
            //  arrange
            var runCount = 0;
            var factory = () => Task.FromResult(++runCount);
            var expired = DateTime.UtcNow + ttl;

            //  test
            while (expired > DateTime.UtcNow)
            {
                await svc.GetOrCreateAsync(factory);
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
            svc.Set(value);
            var actual1 = svc.TryGetValue(out var v1);
            Thread.Sleep(ttl);
            var actual2 = svc.IsExpired;

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
            svc.Set(value);

            //  test
            var actual1 = svc.TryGetValue(out var v1);
            Thread.Sleep(ttl);
            var actual2 = svc.TryGetValue(out var v2);

            //  assert
            Assert.Equal(value, v1);
            Assert.True(actual1);
            Assert.False(actual2);
        }
    }
}