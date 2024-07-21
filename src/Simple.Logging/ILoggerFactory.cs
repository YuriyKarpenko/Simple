using System;

namespace Simple.Logging
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(Type source);
        ILogger CreateLogger<T>();
    }
}