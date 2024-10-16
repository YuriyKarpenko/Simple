using System;

namespace Simple.Logging
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(string source);
        ILogger CreateLogger(Type source);
        ILogger CreateLogger<T>();
    }
}