using Simple.DI;

namespace Test.DI
{
    public class ResolverTests
    {
        private const string key = "test key";
        private readonly Resolver<string> _svc;
        public ResolverTests()
        {
            _svc = new Svc();
        }


        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        [InlineData(8)]
        public void Register(int regCount)
        {
            //  arrange
            Func<object> fact = () => key;

            //  tst
            while (regCount-- > 0)
            {
                _svc.Register(key, () => $"{key} {regCount}");
            }
            _svc.Register(key, fact);
            var actual = _svc.GetService(key);

            //  assert
            Assert.Equal(key, actual);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TryRegister(bool expected)
        {
            //  assert
            Func<object> facttory = () => key;
            if (!expected)
            {
                _svc.TryRegister(key, facttory);
            }

            //  test
            var actual = _svc.TryRegister(key, facttory);

            //  assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Multithtead()
        {
            //  arrange
            var rnd = new Random();
            var keys = Enumerable.Range(0, 100).Select(i => $"key {i}").ToList();

            //  test
            var tasks = keys.Select(i => Task.Run(() =>
            {
                Thread.Sleep(rnd.Next(100));
                _svc.Register(i, () => i);
            }));
            Task.WhenAll(tasks).Wait();

            //  assert
            keys.ForEach(i => Assert.Equal(i, _svc.GetService(i)));
        }

        private class Svc : Resolver<string>
        {
            public Svc() : base(null) { }
        }
    }
}