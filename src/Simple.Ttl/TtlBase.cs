using System;

namespace Simple.Ttl;

public abstract class TtlBase(TimeSpan ttl)
{
    protected readonly object _lock = new object();
    private DateTime? _expired = null;


    public bool IsExpired => !_expired.HasValue || _expired.Value < DateTime.UtcNow;

    /// <summary> Update ttl & create value from <paramref name="factory"/>. </summary>
    /// <typeparam name="T"><paramref name="factory"/> result type</typeparam>
    /// <param name="factory">Factory method for get/create value</param>
    /// <returns><paramref name="factory"/> result value</returns>
    protected T EnsureValue<T>(Func<T> factory)
    {
        lock (_lock)
        {
            _expired = DateTime.UtcNow + ttl;
            return factory();
        }
    }
}