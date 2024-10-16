using System;

namespace Simple.Logging.Messages
{
    public interface ILogMessage
    {
        DateTime Created { get; }
        string LogSource { get; }
        LogLevel Level { get; }
        string? State { get; }
        Exception? Exception { get; }
    }
}