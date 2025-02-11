using System;

using Simple.Logging.Configuration;
using Simple.Logging.Messages;
using Simple.Logging.Scope;

namespace Simple.Logging;

public class LogManager
{
    public static LogLevel DefaultMinLevel
    {
        get => Options.LogLevel.Default;
        set => Options.LogLevel.Default = value;
    }

    public static ILoggerFactory LoggerFactory { get; set; } = new DefaultLoggerFactory();

    public static ILogOptions Options => LogOptions.Instance;

    public static ILogMessageFactory MessageFactory { get; set; } = new DefaultLogMessageFactory();

    public static IExternalScopeProvider ScopeProvider { get; set; } = new DefaultScopeProvider();

    //  filtering helpers
    public static Func<LogLevel, string, bool> FilterIn => Options.FilterIn;
}