using System;

using Simple.Logging.Configuration;
using Simple.Logging.Messages;

namespace Simple.Logging;

public class LogManager
{
    public static LogLevel DefaultMinLevel
    {
        get => Options.Default.MinLevel;
        set => Options.Default.MinLevel = value;
    }

    public static ILoggerFactory LoggerFactory { get; set; } = new DefaultLoggerFactory();

    public static ILogOptions Options => LogOptions.Instance;

    public static ILogMessageFactory MessageFactory { get; set; } = new DefaultLogMessageFactory();

    //  filtering helpers
    public static Func<LogLevel, string, bool> FilterIn => Options.FilterIn;
}