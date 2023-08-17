using System;

using Simple.Logging.Messages;

namespace Simple.Logging
{
    public class Logger : ILogger
    {
        protected readonly string _logSource;
        public Logger(string logSource)
        {
            _logSource = logSource;
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel level)
            => LogManager.FilterIn(level, _logSource);

        /// <inheritdoc />
        public ILogMessage? Log<TState>(LogLevel logLevel, TState state, Exception? exception, Func<TState, Exception?, string>? formatter)
        {
            if (IsEnabled(logLevel))
            {
                var e = LogManager.MessageFactory.CreateMessage(_logSource, logLevel, state, exception, formatter);
                SendMessage(e);
                return e;
            }
            return null;
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) where TState : notnull
            => throw new NotImplementedException();


        protected virtual void SendMessage(ILogMessage message)
            => Logging.LoggingBus.Instance.Push(message);
    }
}