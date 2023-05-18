using System;

namespace Simple.Ttl
{
    public class TtlLazy<T> : TtlValue<T>
    {
        private readonly Func<T> _factory;

        public TtlLazy(TimeSpan ttl, Func<T> factory) : base(ttl)
        {
            _factory = factory;
        }

        public T Value => GetOrCreate(_factory);
    }
}