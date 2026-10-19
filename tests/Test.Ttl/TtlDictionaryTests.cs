using Simple.Ttl;

namespace Test.Ttl
{
    public class TtlDictionaryTests
    {
        private const           string                Key    = "test key";
        private static readonly TimeSpan              ttl    = TimeSpan.FromMilliseconds(300);
        private static readonly TimeSpan              margin = TimeSpan.FromMilliseconds(50); //  safety over the ttl boundary
        private readonly        TtlDictionary<string> svc    = new(ttl);

        [Fact]
        public void GetOrCreate()
        {
            //  arrange
            var expected = 0;
            var factory  = (string s) => ++expected;

            //  test
            var actual = svc.GetOrCreate(Key, factory);
            Thread.Sleep(ttl + margin);
            var actual2 = svc.TryGetValue(Key, out int _);

            //  assert
            Assert.Equal(expected, actual);
            Assert.False(actual2);
        }

        [Fact]
        public void GetOrCreate_Parallel()
        {
            //  arrange
            var expected = 0;

            int factory(string s)
            {
                Thread.Sleep(100);
                return ++expected;
            }

            //  test
            Parallel.Invoke(
                () => svc.GetOrCreate(Key, factory),
                () => svc.GetOrCreate(Key, factory),
                () => svc.GetOrCreate(Key, factory),
                () => svc.GetOrCreate(Key, factory),
                () => svc.GetOrCreate(Key, factory),
                () => svc.GetOrCreate(Key, factory),
                () => svc.GetOrCreate(Key, factory),
                () => svc.GetOrCreate(Key, factory),
                () => svc.GetOrCreate(Key, factory),
                () => svc.GetOrCreate(Key, factory)
            );
            var actual = svc.GetOrCreate(Key, factory);

            //  assert
            Assert.Equal(1, actual);
        }

        [Theory]
        [InlineData(false, 10, 1)]
        [InlineData(false, 20, 1)]
        [InlineData(true, 10, 1)]
        [InlineData(true, 200, 1)]
        public async Task GetOrCreateAsync_Parallel(bool isRunFirs, int iterations, int expected)
        {
            //  arrange: a wide window - dispatching all the actions must never cross the ttl boundary,
            //  so the strict single-flight assertion below stays deterministic
            var svc      = new TtlDictionary<string>(TimeSpan.FromSeconds(2));
            var runCount = 0;

            async Task<int> factory(string _)
            {
                await Task.Delay(100);
                return ++runCount;
            }

            var actions = Enumerable
                .Range(0, iterations)
                .Select<int, Action>(_ => async () => await svc.GetOrCreateAsync(Key, factory))
                .ToArray();

            //  test
            if (isRunFirs)
            {
                //  warm-up: the class is already initialized
                _ = await svc.GetOrCreateAsync(Key, factory);
            }
            Parallel.Invoke(actions);
            var actual = await svc.GetOrCreateAsync(Key, factory);

            //  assert: everyone shares one in-flight computation (single-flight)
            Assert.Equal(expected, actual);
            Assert.Equal(expected, runCount);
        }

        [Fact]
        public void Set()
        {
            //  arrange
            var expected = 16;

            //  test
            var actual  = svc.Set(Key, expected);
            var actual1 = svc.TryGetValue(Key, out int _);
            Thread.Sleep(ttl + margin);
            var actual2 = svc.TryGetValue(Key, out int _);

            //  assert
            Assert.Equal(expected, actual);
            Assert.True(actual1);
            Assert.False(actual2);
        }

        [Fact]
        public void GetOrCreate_StaleWhileRevalidate_ServesStale()
        {
            //  arrange
            var svc      = new TtlDictionary<string>(ttl, true);
            var runCount = 0;

            //  test
            var actual0 = svc.GetOrCreate(Key, _ => ++runCount);
            Thread.Sleep(ttl);
            var stale = svc.GetOrCreate(Key, _ => ++runCount);

            //  assert
            Assert.Equal(1, actual0);
            Assert.Equal(1, stale);
            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(3);
            var actual   = stale;
            while (actual != 2 && DateTime.UtcNow < deadline)
            {
                actual = svc.GetOrCreate(Key, _ => ++runCount);
                Thread.Sleep(10);
            }
            Assert.Equal(2, actual);
            Assert.Equal(2, runCount);
        }

        [Fact]
        public void HasKey()
        {
            //  arrange
            var missingKey = "missing key";

            //  test
            var actual0 = svc.HasKey(Key);
            svc.GetOrCreate(Key, _ => 1);
            var actual1 = svc.HasKey(Key);
            var actual2 = svc.HasKey(missingKey);

            //  assert
            Assert.False(actual0);
            Assert.True(actual1);
            Assert.False(actual2);
        }

        [Fact]
        public void TryGetValue_MissingKey_ReturnsFalse()
        {
            //  test
            var actual = svc.TryGetValue("missing key", out int _);

            //  assert
            Assert.False(actual);
        }

        [Fact]
        public void GetOrCreate_KeyWithAnotherValueType_ThrowsArgumentException()
        {
            //  arrange
            svc.GetOrCreate(Key, _ => 1);

            //  test
            var ex = Assert.Throws<ArgumentException>(() => svc.GetOrCreate(Key, _ => "other"));

            //  assert
            Assert.Contains(Key, ex.Message);
        }

        [Fact]
        public void Ttl_ReturnsConfiguredValue()
        {
            //  test
            var actual = svc.Ttl;

            //  assert
            Assert.Equal(ttl, actual);
        }
    }
}