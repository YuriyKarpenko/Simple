using System;

namespace Simple.Logging
{
    public interface ILoggerFactory
    {
        ILogger GetLogger(Type source);
        ILogger GetLogger<T>();
    }
}
