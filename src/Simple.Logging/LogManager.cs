using System;

using Simple.Logging.Configuration;
using Simple.Logging.Messages;

namespace Simple.Logging
{
    public class LogManager
    {
        public static LogLevel DefaultMinLevel { get; set; } = LogLevel.Error;

        public static ILoggerFactory LoggerFactory { get; set; } = new DefaultLoggerFactory();

        public static ILogOptions Options { get; set; } = LogOptions.Instance;

        public static ILogMessageFactory MessageFactory { get; set; } = new DefaultLogMessageFactory();

        //  filtering helpers
        public static Func<LogLevel, string, bool> FilterIn => Options.FilterOptions.FilterIn;
    }
}