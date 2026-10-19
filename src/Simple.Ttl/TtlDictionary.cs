using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Simple.Ttl;

/// <summary> Key-value ttl-cache: every key owns an independent value with its own ttl window. </summary>
/// <exception cref="ArgumentException"/>
public class TtlDictionary<TKey>(TimeSpan ttl, bool staleWhileRevalidate)
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TtlBase> _cache = [];

    /// <summary> Initializes the cache with stale-while-revalidate disabled. </summary>
    public TtlDictionary(TimeSpan ttl) : this(ttl, false)
    {
    }

    /// <summary> The ttl applied to every new value of this dictionary. </summary>
    public TimeSpan Ttl => ttl;


    /// <summary> Returns the value of <paramref name="key"/> or creates it with <paramref name="factory"/> after expiry. </summary>
    public T GetOrCreate<T>(TKey key, Func<TKey, T> factory)
        => EnsureTtlValue<T>(key).GetOrCreate(() => factory(key));

    /// <summary> Async GetOrCreate: concurrent callers for one key share a single factory task. </summary>
    public Task<T> GetOrCreateAsync<T>(TKey key, Func<TKey, Task<T>> factory)
        => EnsureTtlValue<T>(key).GetOrCreateAsync(() => factory(key));

    /// <summary> Stores a fresh value under <paramref name="key"/> and restarts its ttl; returns the value. </summary>
    public T Set<T>(TKey key, T value)
        => EnsureTtlValue<T>(key).Set(value);

    /// <summary> True if a value is stored under the key (expired values still count until refreshed). </summary>
    public bool HasKey(TKey key)
        => _cache.ContainsKey(key);

    /// <summary> Reads a fresh value by key; missing or expired is a miss (false). </summary>
    public bool TryGetValue<T>(TKey key, out T? value)
    {
        value = default;
        return _cache.TryGetValue(key, out var tb) && tb is TtlValue<T> tv && tv.TryGetValue(out value);
    }


    /// <summary> Returns the ttl-wrapper stored for the key, creating it on first use;
    /// throws when the key already holds a value of another type. </summary>
    protected virtual TtlValue<T> EnsureTtlValue<T>(TKey key)
    {
        var tb = _cache.GetOrAdd(key, _ => new TtlValue<T>(ttl, staleWhileRevalidate));
        return tb is TtlValue<T> tv
            ? tv
            : throw new ArgumentException($"key {key} related onto invalid value: {tb}");
    }
}