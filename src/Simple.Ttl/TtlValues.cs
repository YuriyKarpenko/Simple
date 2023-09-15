using System;
using System.Threading.Tasks;

namespace Simple.Ttl
{
    public class TtlValues : TtlDictionary<Type>
    {
        public TtlValues(TimeSpan timeout) : base(timeout)
        {
        }

        public T GetOrCreate<T>(Func<T> factory)
            => EnsureTtlValue<T>(typeof(T)).GetOrCreate(factory);

        public Task<T> GetOrCreateAsync<T>(Func<Task<T>> factory)
            => EnsureTtlValue<T>(typeof(T)).GetOrCreateAsync(factory);
    }
}