using System;
using System.Threading.Tasks;

namespace Simple.Ttl
{
    public class TtlValue<T> : TtlBase
    {
        private T? value = default;

        public TtlValue(TimeSpan ttl) : base(ttl)
        {
        }

        public T GetOrCreate(Func<T> factory)
            => IsExpired
            ? value = EnsureValue(factory)
            : value!;

        //  TODO: need tests in multithread
        public async Task<T> GetOrCreateAsync(Func<Task<T>> factory)
            => IsExpired
            ? value = await EnsureValue(factory)
            : value!;

        public T Set(T value)
            => this.value = EnsureValue(() => value);

        public bool TryGetValue(out T? value)
        {
            var expired = IsExpired;
            value = expired ? default : this.value;
            return !expired;
        }
    }
}