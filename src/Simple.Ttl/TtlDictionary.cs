using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Simple.Ttl
{
    public class TtlDictionary<TKey>
    {
        private readonly ConcurrentDictionary<TKey, TtlBase> _cache = new ConcurrentDictionary<TKey, TtlBase>();
        private readonly TimeSpan _ttl;
        public TtlDictionary(TimeSpan ttl)
        {
            _ttl = ttl;
        }


        public TimeSpan Ttl => _ttl;

        public T GetOrCreate<T>(TKey key, Func<TKey, T> factory)
            => EnsureTtlValue<T>(key).GetOrCreate(() => factory(key));

        public Task<T> GetOrCreateAsync<T>(TKey key, Func<TKey, Task<T>> factory)
            => EnsureTtlValue<T>(key).GetOrCreateAsync(() => factory(key));

        public T Set<T>(TKey key, T value)
            => EnsureTtlValue<T>(key).Set(value);

        public bool HasKey(TKey key)
            => _cache.ContainsKey(key);

        public bool TryGetValue<T>(TKey key, out T? value)
        {
            value = default;
            return _cache.TryGetValue(key, out var ttl) && ttl is TtlValue<T> t && t!.TryGetValue(out value);
        }


        protected virtual TtlValue<T> EnsureTtlValue<T>(TKey key)
            => (TtlValue<T>)_cache.GetOrAdd(key, (TKey k) => new TtlValue<T>(_ttl));
    }
}
