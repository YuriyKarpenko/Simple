using System;
using System.Collections.Generic;

namespace Simple.Ttl
{
    public class TtlDictionary<T>
    {
        private readonly Dictionary<string, TtlValue<T>> _cache = new Dictionary<string, TtlValue<T>>(StringComparer.OrdinalIgnoreCase);
        private readonly TimeSpan _ttl;
        public TtlDictionary(TimeSpan ttl)
        {
            _ttl = ttl;
        }

        public T GetOrCreate(string key, Func<T> factory)
            => EnsureValue(key).GetOrCreate(factory);

        public T Set(string key, T value)
            => EnsureValue(key).Set(value);

        public bool TryGetValue(string key, out T? value)
        {
            value = default;
            return _cache.TryGetValue(key, out var ttlValue) && ttlValue.TryGetValue(out value);
        }


        private TtlValue<T> EnsureValue(string key)
        {
            if (!_cache.TryGetValue(key, out var ttlValue))
            {
                _cache[key] = ttlValue = new TtlValue<T>(_ttl);
            }
            return ttlValue;
        }
    }
}
