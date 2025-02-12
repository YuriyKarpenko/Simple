using System;

using Simple.Logging.Messages;

namespace Simple.Logging;

public class Logger : ILogger
{
    protected readonly ILogMessageBus<LogMessage> _messageBus;
    protected readonly string _logSource;
    public Logger(string logSource, ILogMessageBus<LogMessage> messageBus)
    {
        _messageBus = messageBus;
        _logSource = logSource;
    }

    #region ILogger

    /// <inheritdoc />
    public bool IsEnabled(LogLevel level)
        => LogManager.FilterIn(level, _logSource);

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (IsEnabled(logLevel))
        {
            var e = LogManager.MessageFactory.CreateMessage(_logSource, logLevel, state, exception, formatter);
            _messageBus.Push(e);
        }
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state)
        => LogManager.ScopeProvider.Push(state);

    #endregion
}