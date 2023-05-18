using System;

namespace Simple.Logging.Messages
{
    public class DefaultLogMessageFactory : ILogMessageFactory
    {
        public static LogMessageString CreateEntry<TState>(Type logSource, LogLevel level, int eventId, TState state, Exception? exception)
            => new LogMessageString(logSource, level, state?.ToString() ?? string.Empty, exception);

        ILogMessage ILogMessageFactory<ILogMessage>.CreateMessage<TState>(Type logSource, LogLevel level, int eventId, TState state, Exception? exception)
            => CreateEntry(logSource, level, eventId, state, exception);
    }
}
