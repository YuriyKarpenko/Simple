using System;

using Simple.Logging.Messages;

namespace Simple.Logging
{
    public interface ILogger
    {
        /// <summary>
        /// Only for compatibility with <see cref="Microsoft.Extensions.Logging"/>
        /// </summary>
        bool IsEnabled(LogLevel level);

        ILogMessage Log<TState>(LogLevel logLevel, int eventId, TState state, Exception? exception);
    }
}
