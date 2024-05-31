using Simple.Ttl;

using Xunit;

namespace Test.Ttl
{
    public class TtlDictionaryTests
    {
        private const string Key = "test key";
        private static readonly TimeSpan ttl = TimeSpan.FromSeconds(1);
        private readonly TtlDictionary<string> svc = new(ttl);

        [Fact]
        public void GetOrCreate()
        {
            //  arrange
            var expected = 0;
            var factory = (string s) => ++expected;

            //  test
            var actual = svc.GetOrCreate(Key, factory);
            Thread.Sleep(ttl);
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

        [Fact]
        public async Task GetOrCreateAsync_Parallel()
        {
            //  arrange
            var expected = 0;
            async Task<int> factory(string s)
            {
                await Task.Delay(100);
                return  ++expected;
            }

            //  test
            Parallel.Invoke(
                async () => await svc.GetOrCreateAsync(Key, factory),
                async () => await svc.GetOrCreateAsync(Key, factory),
                async () => await svc.GetOrCreateAsync(Key, factory),
                async () => await svc.GetOrCreateAsync(Key, factory),
                async () => await svc.GetOrCreateAsync(Key, factory),
                async () => await svc.GetOrCreateAsync(Key, factory),
                async () => await svc.GetOrCreateAsync(Key, factory),
                async () => await svc.GetOrCreateAsync(Key, factory),
                async () => await svc.GetOrCreateAsync(Key, factory),
                async () => await svc.GetOrCreateAsync(Key, factory),
                async () => await svc.GetOrCreateAsync(Key, factory),
                async () => await svc.GetOrCreateAsync(Key, factory),
                async () => await svc.GetOrCreateAsync(Key, factory)
                );
            var actual = await svc.GetOrCreateAsync(Key, factory);

            //  assert
            Assert.Equal(1, actual);
        }

        [Fact]
        public void Set()
        {
            //  arrange
            var expected = 16;

            //  test
            var actual = svc.Set(Key, expected);
            var actual1 = svc.TryGetValue(Key, out int _);
            Thread.Sleep(ttl);
            var actual2 = svc.TryGetValue(Key, out int _);

            //  assert
            Assert.Equal(expected, actual);
            Assert.True(actual1);
            Assert.False(actual2);
        }
    }
}
