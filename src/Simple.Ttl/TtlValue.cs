using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Ttl;

public class TtlValue<T> : TtlBase
{
    protected T _value = default!; //  начальное значение попадает наружу только через GetOrCreate()

    public TtlValue(TimeSpan ttl) : base(ttl)
    {
    }

    public T GetOrCreate(Func<T> factory)
    {
        lock (_lock)
        {
            return IsExpired
                ? _value = EnsureValue(factory)
                : _value;
        }
    }

    public T Set(T value)
        => _value = EnsureValue(() => value);

    public bool TryGetValue(out T? value)
    {
        var expired = IsExpired;
        value = expired ? default : _value;
        return !expired;
    }

    #region async

    private Task<T>? _taskValue = null;

    public Task<T> GetOrCreateAsync(Func<Task<T>> factory)
    {
        if (_taskValue == null && IsExpired || _taskValue?.Exception != null)
        {
            lock (_lock)
            {
                Interlocked.Exchange(ref _taskValue, factory().ContinueWith(SetAsync));
            }
        }
        return _taskValue ?? Task.FromResult(_value);
    }

    public T SetAsync(Task<T> taskValue)
    {
        if (taskValue.Status == TaskStatus.RanToCompletion)
        {
            Set(taskValue.Result);
        }
        Interlocked.Exchange(ref _taskValue, null);
        return _value;
    }

    #endregion
}