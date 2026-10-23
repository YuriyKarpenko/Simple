using System;
using System.Threading.Tasks;

namespace Simple.Ttl;

/// <summary> Ttl-cache keyed by value type: one value per T, shared ttl and refresh mode. </summary>
public class TtlValues(TimeSpan ttl, bool staleWhileRevalidate) : TtlDictionary<Type>(ttl, staleWhileRevalidate)
{
    /// <summary> Initializes the cache with stale-while-revalidate disabled. </summary>
    public TtlValues(TimeSpan ttl) : this(ttl, false)
    {
    }

    /// <summary> Returns the value of type <typeparamref name="T"/> or creates it after expiry. </summary>
    public T GetOrCreate<T>(Func<T> factory)
        => EnsureTtlValue<T>(typeof(T)).GetOrCreate(factory);

    /// <summary> Async GetOrCreate for the value of type <typeparamref name="T"/>. </summary>
    public Task<T> GetOrCreateAsync<T>(Func<Task<T>> factory)
        => EnsureTtlValue<T>(typeof(T)).GetOrCreateAsync(factory);
}