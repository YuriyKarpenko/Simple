using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Ttl
{
    public class TtlValue<T> : TtlBase
    {
        protected T? value = default;

        public TtlValue(TimeSpan ttl) : base(ttl)
        {
        }

        public T GetOrCreate(Func<T> factory)
        {
            lock (_lock)
            {
                return IsExpired
                    ? value = EnsureValue(factory)
                    : value!;
            }
        }

        public T Set(T value)
            => this.value = EnsureValue(() => value);

        public bool TryGetValue(out T? value)
        {
            var expired = IsExpired;
            value = expired ? default : this.value;
            return !expired;
        }

        #region async

        private Task<T?>? _taskValue = null;

        public Task<T?> GetOrCreateAsync(Func<Task<T>> factory)
        {
            //  if the Value is expired and not in process getting Value
            if (IsExpired && _taskValue == null)
            {
                lock (_lock)
                {
                    Interlocked.Exchange(ref _taskValue, factory().ContinueWith(SetAsync));
                }
            }
            return _taskValue ?? Task.FromResult(value);

            T? SetAsync(Task<T> taskValue)
            {
                if (taskValue.Status == TaskStatus.RanToCompletion)
                {
                    Set(taskValue.Result);
                }
                Interlocked.Exchange(ref _taskValue, null);
                return value;
            }
        }

        #endregion
    }
}