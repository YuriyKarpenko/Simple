using System;

namespace Simple.Ttl
{
    public abstract class TtlBase
    {
        protected readonly object _lock = new object();
        private readonly TimeSpan _ttl;
        private DateTime? expired = null;

        public TtlBase(TimeSpan ttl)
        {
            _ttl = ttl;
        }


        public bool IsExpired => !expired.HasValue || expired.Value < DateTime.UtcNow;

        /// <summary> Update ttl & create value from <paramref name="factory"/>. </summary>
        /// <typeparam name="T"><paramref name="factory"/> result type</typeparam>
        /// <param name="factory">Factory method for get/create value</param>
        /// <returns><paramref name="factory"/> result value</returns>
        protected T EnsureValue<T>(Func<T> factory)
        {
            lock (_lock)
            {
                expired = DateTime.UtcNow + _ttl;
                return factory();
            }
        }
    }
}