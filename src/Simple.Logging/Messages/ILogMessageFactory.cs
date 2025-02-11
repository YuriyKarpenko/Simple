using System;

namespace Simple.Logging.Messages;

public interface ILogMessageFactory
{
    ILogMessage CreateMessage<TState>(string logSource, LogLevel level, TState state, Exception? exception, Func<TState, Exception?, string>? formatter = null);
    string CreateScopes();
    string ToStringWithoutLevel(ILogMessage message);
}