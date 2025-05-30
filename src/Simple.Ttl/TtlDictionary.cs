using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Simple.Ttl;

/// <exception cref="ArgumentException"/>
public class TtlDictionary<TKey>(TimeSpan ttl) where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TtlBase> _cache = new ConcurrentDictionary<TKey, TtlBase>();

    public TimeSpan Ttl => ttl;


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
        return _cache.TryGetValue(key, out var tb) && tb is TtlValue<T> tv && tv.TryGetValue(out value);
    }


    protected virtual TtlValue<T> EnsureTtlValue<T>(TKey key)
    {
        var tb = _cache.GetOrAdd(key, _ => new TtlValue<T>(ttl));
        return tb is TtlValue<T> tv
            ? tv
            : throw new ArgumentException($"key {key} related onto invalid value: {tb}");
    }
}