using System;
using System.Diagnostics;

namespace Simple.Ttl;

/// <summary> Base of ttl-wrappers: one lock for value publication and a monotonic expiry clock. </summary>
public abstract class TtlBase(TimeSpan ttl, bool staleWhileRevalidate)
{
    protected readonly object _lock                 = new();
    protected readonly bool   _staleWhileRevalidate = staleWhileRevalidate;
    //  monotonic clock (Stopwatch ticks): expiry ignores wall clock jumps (NTP, DST, manual changes)
    protected        long? _expired;
    private readonly long  _ttlTicks = (long)(ttl.TotalSeconds * Stopwatch.Frequency);

    /// <summary> True while no value exists yet, or the ttl passed since the last refresh. </summary>
    public bool IsExpired => !_expired.HasValue || _expired.Value < Stopwatch.GetTimestamp();

    /// <summary> Update ttl &amp; create value from <paramref name="factory"/>. </summary>
    /// <typeparam name="T"><paramref name="factory"/> result type</typeparam>
    /// <param name="factory">Factory method for get/create value</param>
    /// <returns><paramref name="factory"/> result value</returns>
    protected T EnsureValue<T>(Func<T> factory)
    {
        lock (_lock)
        {
            var value = factory();
            _expired = Stopwatch.GetTimestamp() + _ttlTicks;
            return value;
        }
    }
}