using System;

using Simple.Logging.Configuration;
using Simple.Logging.Logging;
using Simple.Logging.Observers;

namespace Simple.Logging
{
    public static class OptionExtensions
    {
        public static ILogger Log(this IEnableLogger logger)
            => LogManager.LoggerFactory.CreateLogger(logger.GetType());

        public static ILogger CreateLogger(this IServiceProvider _, Type src)
            => LogManager.LoggerFactory.CreateLogger(src);
        public static ILogger CreateLogger<T>(this IServiceProvider _)
            => LogManager.LoggerFactory.CreateLogger<T>();


        public static ILogOptions AddConsole(this ILogOptions o)
        {
            LoggingBus.Instance.Subscribe(new ObserverConsole());
            return o;
        }

        public static ILogOptions AddDebug(this ILogOptions o)
        {
            LoggingBus.Instance.Subscribe(new ObserverDebug());
            return o;
        }
    }
}