﻿using System;

using Simple.Logging.Configuration;
using Simple.Logging.Messages;

namespace Simple.Logging;

public class LogManager
{
    public static LogLevel DefaultMinLevel
    {
        get => Options.LogLevel.Default;
        set => Options.LogLevel.Default = value;
    }

    public static ILoggerFactory LoggerFactory { get; set; } = new LoggerFactory();

    public static ILogOptions Options => LogOptions.Instance;

    public static ILogMessageFactory MessageFactory { get; set; } = new LogMessageFactory();

    public static IExternalScopeProvider ScopeProvider { get; set; } = new ExternalScopeProvider();

    //  filtering helpers
    public static Func<LogLevel, string, bool> FilterIn => Options.FilterIn;
}