using System;
using System.Threading.Tasks;

namespace Simple.Ttl;

/// <summary> Single value with ttl: blocking factory at cold start, optional stale-while-revalidate
/// (stale read plus background refresh). </summary>
public class TtlValue<T>(TimeSpan ttl, bool staleWhileRevalidate) : TtlBase(ttl, staleWhileRevalidate)
{
    protected T _value = default!;
    //  serializes blocking factory runs so a slow factory does not hold _lock
    private readonly object _factoryLock = new();

    /// <summary> Initializes the value without stale-while-revalidate. </summary>
    public TtlValue(TimeSpan ttl) : this(ttl, false)
    {
    }

    //  blocking path: cold start serializes on a dedicated factory gate (_lock stays free for Set/async);
    //  SWR mode returns the stale value and starts exactly one background refresh

    /// <summary> Returns the current value, or runs <paramref name="factory"/> after expiry -
    /// exactly one factory run per expiry window even under concurrency. </summary>
    /// <param name="factory">Creates a fresh value; runs outside the value lock. </param>
    public T GetOrCreate(Func<T> factory)
    {
        //  hot path: freshness & SWR decision under _lock only - the factory gate is never taken here
        lock (_lock)
        {
            if (!IsExpired)
            {
                return _value;
            }
            if (_staleWhileRevalidate && _expired.HasValue)
            {
                if (_taskValue == null)
                {
                    var refresh = Task.Run(factory);
                    _taskValue = refresh;
                    _          = refresh.ContinueWith(SetAsync);
                }
                return _value;
            }
        }
        //  factory gate: only cold-start callers contend here - single-flight is enforced by the re-check below
        lock (_factoryLock)
        {
            lock (_lock)
            {
                if (!IsExpired)
                {
                    return _value; //  re-check: another caller refreshed while we waited on the gate
                }
            }
            var value = factory();
            _value = EnsureValue(() => value);
            return value;
        }
    }

    /// <summary> Stores a fresh value and restarts its ttl; returns the stored value. </summary>
    public T Set(T value)
    {
        lock (_lock)
        {
            _value = EnsureValue(() => value);
            return _value;
        }
    }

    //  serves only fresh values: expired (incl. stale in SWR mode) is a miss

    /// <summary> Reads the value only while it is fresh; expired is a miss (false, default value). </summary>
    public bool TryGetValue(out T? value)
    {
        var expired = IsExpired;
        value = expired ? default : _value;
        return !expired;
    }

    #region async

    //  single-flight marker: the in-flight factory task itself; null = no refresh in flight
    private Task<T>? _taskValue;

    /// <summary> Async counterpart of GetOrCreate: concurrent callers join one in-flight factory task
    /// (single-flight); a factory exception reaches the awaiting caller and the next call retries. </summary>
    /// <param name="factory">Creates a fresh value; runs under the value lock until its first await. </param>
    public Task<T> GetOrCreateAsync(Func<Task<T>> factory)
    {
        //  SWR: a value has existed at least once -> serve stale, kick one refresh in background;
        //  before the first value this block falls through to the cold/join path below
        if (_staleWhileRevalidate)
        {
            lock (_lock)
            {
                if (_expired.HasValue)
                {
                    if (IsExpired && _taskValue == null)
                    {
                        var refresh = factory();
                        _taskValue = refresh;
                        _          = refresh.ContinueWith(SetAsync);
                    }
                    return Task.FromResult(_value);
                }
            }
        }
        lock (_lock)
        {
            if (_taskValue?.IsFaulted == true)
            {
                _taskValue = null; //  faulted in-flight is abandoned - the next call retries
            }
            if (_taskValue == null)
            {
                if (!IsExpired)
                {
                    return Task.FromResult(_value);
                }
                var task = factory();
                _taskValue = task;
                _          = task.ContinueWith(SetAsync);
                return task; //  the raw task itself: factory faults reach the awaiting caller
            }
            return _taskValue; //  join the computation in flight (single-flight)
        }
    }

    //  ContinueWith callback: store a fresh value, observe faults,
    //  reset the single-flight marker if it is still ours
    private void SetAsync(Task<T> taskValue)
    {
        lock (_lock)
        {
            if (taskValue.Status == TaskStatus.RanToCompletion)
            {
                Set(taskValue.Result);
            }
            else if (taskValue.IsFaulted)
            {
                _ = taskValue.Exception;
            }
            if (ReferenceEquals(_taskValue, taskValue))
            {
                _taskValue = null;
            }
        }
    }

    #endregion

}