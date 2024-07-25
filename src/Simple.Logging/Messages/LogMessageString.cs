using System;

namespace Simple.Logging.Messages
{
    public class LogMessageString : ILogMessage
    {
        public LogMessageString(string logSource, LogLevel level, string? state, Exception? exception)
        {
            Created = DateTime.UtcNow;
            Exception = exception;
            Level = level;
            LogSource = logSource;
            State = state;
        }

        public DateTime Created { get; }
        public string LogSource { get; }
        public LogLevel Level { get; }
        public string? State { get; }
        public Exception? Exception { get; }


        public override string ToString()
            => $"{Level,-9} {State} [{LogSource}]";
    }
}