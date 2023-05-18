using System;

namespace Simple.Logging.Messages
{
    public abstract class LogMessageBase : ILogMessage
    {
        public LogMessageBase(Type logSource, LogLevel level, Exception? exception)
        {
            Exception = exception;
            Level = level;
            LogSource = logSource;
        }

        public Type LogSource { get; }
        public LogLevel Level { get; }
        public Exception? Exception { get; }

        public virtual string CategoryName => LogSource.FullName;
        public virtual string Name => LogSource.Name;

        public override string ToString()
            => $"{Level,-9} - {Name,-15}: {Exception}";
    }
}