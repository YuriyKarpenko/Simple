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
