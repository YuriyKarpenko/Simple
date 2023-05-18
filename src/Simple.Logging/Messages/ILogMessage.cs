using System;

namespace Simple.Logging.Messages
{
    public interface ILogMessage
    {
        Type LogSource { get; }
        LogLevel Level { get; }
        Exception? Exception { get; }

        string ToString();
    }
}
