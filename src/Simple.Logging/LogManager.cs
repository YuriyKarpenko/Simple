using System;

using Simple.Logging.Configuration;
using Simple.Logging.Messages;
using Simple.Logging.Observers;

namespace Simple.Logging
{
    public class LogManager
    {
        public static LogLevel DefaultMinLevel { get; set; } = LogLevel.Error;

        public static ILoggerFactory LoggerFactory { get; set; } = new DefaultLoggerFactory();

        public static ILogOptions Options { get; set; } = LogOptions.Instance;
        public static ILogMessageFactory MessageFactory { get; set; } = new DefaultLogMessageFactory();

        //  for ILogger
        public static Func<ILogMessage, bool> FilterIn { get; set; } = (_) => true;

        //  for ILogObserver
        public static Func<ILogMessage, ILogObserver, bool> FilterOut { get; set; } = (_, _) => true;
    }
}
