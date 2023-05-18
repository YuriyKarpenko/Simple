using System;

namespace Simple.Logging.Messages
{
    public interface ILogMessageFactory<TLogMessage> where TLogMessage : ILogMessage
    {
        TLogMessage CreateMessage<TState>(Type logSource, LogLevel level, int eventId, TState state, Exception? exception);
    }

    public interface ILogMessageFactory : ILogMessageFactory<ILogMessage>
    {
    }
}
