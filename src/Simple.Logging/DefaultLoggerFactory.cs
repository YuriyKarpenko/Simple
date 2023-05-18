using System;
using System.Collections.Concurrent;

using Simple.Logging.Logging;

namespace Simple.Logging
{
    public class DefaultLoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<Type, ILogger> _cache = new();


        /// <inheritdoc />
        public ILogger GetLogger(Type logSource)
        {
            return _cache.GetOrAdd(logSource, key => new DefaultLogger(key));
        }

        /// <inheritdoc />
        public ILogger GetLogger<T>()
            => GetLogger(typeof(T));
    }
}
