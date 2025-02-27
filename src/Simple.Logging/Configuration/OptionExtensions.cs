using System;

using Simple.Logging.Configuration;
using Simple.Logging.Messages;
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


        public static ILogOptions AddConsole(this ILogOptions o, Action<LoggerFilterItem, ConfigurationConsole>? configure = null)
        {
            var observer = new ObserverConsole(o, configure);
            return o.AddObserver(observer);
        }

        public static ILogOptions AddDebug(this ILogOptions o, Action<LoggerFilterItem>? configure = null)
        {
            var observer = new ObserverDebug(o, configure);
            return o.AddObserver(observer);
        }

        public static ILogOptions AddObserver(this ILogOptions o, ILogObserver observer)
        {
            LogMessageBus.Instance.Subscribe(observer);
            return o;
        }

        public static ILogOptions SetMinLevel(this ILogOptions o, LogLevel level)
        {
            o.LogLevel.Default = level;
            return o;
        }
    }
}