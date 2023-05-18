using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Simple.Ttl
{
    public class TtlValues
    {
        private readonly ConcurrentDictionary<Type, TtlBase> _cache = new();
        private readonly TimeSpan _ttl;

        public TtlValues(TimeSpan timeout)
        {
            _ttl = timeout;
        }

        public T GetOrCreate<T>(Func<T> factory)
            => EnsureValue<T>().GetOrCreate(factory);

        public Task<T> GetOrCreateAsync<T>(Func<Task<T>> factory)
            => EnsureValue<T>().GetOrCreateAsync(factory);


        protected virtual TtlValue<T> EnsureValue<T>()
            => (TtlValue<T>)_cache.GetOrAdd(typeof(T), t => new TtlValue<T>(_ttl));
    }
}
