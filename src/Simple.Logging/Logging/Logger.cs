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
            => LogManager.Options.FilterOptions.Default.MinLevel <= level;

        /// <inheritdoc />
        public ILogMessage Log<TState>(LogLevel logLevel, TState state, Exception? exception, Func<TState, Exception?, string>? formatter)
        {
            var e = LogManager.MessageFactory.CreateMessage(_logSource, logLevel, state, exception, formatter);
            if (LogManager.FilterIn(e))
            {
                SendMessage(e);
            }
            return e;
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state) where TState : notnull
            => throw new NotImplementedException();


        protected virtual void SendMessage(ILogMessage message)
            => Logging.LoggingBus.Instance.Push(message);
    }
}