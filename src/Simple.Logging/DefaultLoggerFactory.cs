using System;
using System.Collections.Concurrent;

using Simple.Logging.Logging;

namespace Simple.Logging
{
    public class DefaultLoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<Type, ILogger> _cache = new();


        /// <inheritdoc />
        public ILogger CreateLogger(Type logSource)
        {
            return _cache.GetOrAdd(logSource, key => new DefaultLogger(key));
        }

        /// <inheritdoc />
        public ILogger CreateLogger<T>()
            => CreateLogger(typeof(T));
    }
}
