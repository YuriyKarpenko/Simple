﻿using System;
using System.Runtime.CompilerServices;

namespace Simple.Logging;

public static class LoggerExtensions
{
    public static void CriticalMethod(this ILogger logger, Exception ex, Func<string?> getArgs, Func<string?>? getMethodResult = null, [CallerMemberName] string? methodName = null)
        => LogMethod(logger, LogLevel.Critical, getArgs, getMethodResult, ex, methodName);

    public static void ErrorMethod(this ILogger logger, Exception ex, Func<string?> getArgs, Func<string?>? getMethodResult = null, [CallerMemberName] string? methodName = null)
        => LogMethod(logger, LogLevel.Error, getArgs, getMethodResult, ex, methodName);

    public static void WarningMethod(this ILogger logger, Exception? ex, Func<string?> getArgs, Func<string?>? getMethodResult = null, [CallerMemberName] string? methodName = null)
        => LogMethod(logger, LogLevel.Warning, getArgs, getMethodResult, ex, methodName);

    public static void InfoMethod(this ILogger logger, Func<string?> getArgs, Func<string?>? getMethodResult = null, [CallerMemberName] string? methodName = null)
        => LogMethod(logger, LogLevel.Information, getArgs, getMethodResult, null, methodName);

    public static void DebugMethod(this ILogger logger, Func<string?> getArgs, Func<string?>? getMethodResult = null, [CallerMemberName] string? methodName = null)
        => LogMethod(logger, LogLevel.Debug, getArgs, getMethodResult, null, methodName);

    public static void TraceMethod(this ILogger logger, Func<string?> getArgs, Func<string?>? getMethodResult = null, [CallerMemberName] string? methodName = null)
        => LogMethod(logger, LogLevel.Trace, getArgs, getMethodResult, null, methodName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogMethod(this ILogger logger, LogLevel level, Func<string?> getArgs, Func<string?>? getMethodResult = null, Exception? ex = null, [CallerMemberName] string? methodName = null)
    {
        if (logger.IsEnabled(level))
        {
            logger.Log(level, 0, (methodName, args: getArgs(), getMethodResult), ex, static (state, _) =>
                state.getMethodResult is null
                    ? string.Format(msgFormat_2, state.methodName, state.args)
                    : string.Format(msgFormat_3, state.methodName, state.args, state.getMethodResult())
                    );
        }
    }

    private const string
        msgFormat_2 = "{0}({1})",
        msgFormat_3 = "{0}({1}) => {2}";
}