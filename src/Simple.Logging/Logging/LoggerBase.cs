using System;

using Simple.Logging.Messages;

namespace Simple.Logging.Logging
{
    //public abstract class LoggerBase<TLogMessage> : ILogger where TLogMessage : ILogMessage
    public abstract class LoggerBase : ILogger
    {
        protected readonly Type _logSource;
        public LoggerBase(Type logSoutce)
        {
            _logSource = logSoutce;
        }

        /// <inheritdoc />
        public bool IsEnabled(LogLevel level)
            => LogManager.Options.FilterOptions.Default.MinLevel <= level;

        /// <inheritdoc />
        public virtual ILogMessage Log<TState>(LogLevel logLevel, int eventId, TState state, Exception? exception)
        {
            var e = CreateMessage(logLevel, eventId, state, exception);
            if (LogManager.FilterIn(e))
            {
                SendMessage(e);
            }
            return e;
        }


        protected virtual ILogMessage CreateMessage<TState>(LogLevel logLevel, int eventId, TState state, Exception? exception)
            => LogManager.MessageFactory.CreateMessage(_logSource, logLevel, eventId, state, exception);

        protected virtual void SendMessage(ILogMessage message)
            => LoggingBus<ILogMessage>.Instance.Push(message);
    }
}