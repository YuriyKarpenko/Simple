using System;
using System.Diagnostics;

namespace Simple.Logging.Messages;

[DebuggerDisplay("{Level,-9} {State} [{LogSource}]")]
public struct ILogMessage
{
    public ILogMessage(string logSource, LogLevel level, string? message, Exception? exception)
    {
        Created = DateTime.UtcNow;
        Level = level;
        LogSource = logSource;
        State = message;
        Exception = exception;
    }

    public DateTime Created { get; }
    public string LogSource { get; }
    public LogLevel Level { get; }
    public string? State { get; }
    public Exception? Exception { get; }
}