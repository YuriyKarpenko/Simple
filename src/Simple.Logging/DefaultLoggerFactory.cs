using System;
using System.Collections.Concurrent;

namespace Simple.Logging
{
    public class DefaultLoggerFactory : ILoggerFactory
    {
        private readonly ConcurrentDictionary<string, ILogger> _cache = new();


        /// <inheritdoc />
        public ILogger CreateLogger(string logSource)
        {
            return _cache.GetOrAdd(logSource, key => new Logger(key));
        }

        /// <inheritdoc />
        public ILogger CreateLogger(Type logSource)
        {
            var t = Throw.IsArgumentNullException(logSource, nameof(logSource));
            return CreateLogger(t.FullName ?? t.Name);
        }

        /// <inheritdoc />
        public ILogger CreateLogger<T>()
            => CreateLogger(typeof(T));
    }
}