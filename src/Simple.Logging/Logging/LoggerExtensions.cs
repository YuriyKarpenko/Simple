using System;
using System.Runtime.CompilerServices;

namespace Simple.Logging
{
    public static class LoggerExtensions
    {
        public static void Trace(this ILogger logger, string message, Exception? exception = null)
            => logger.Log(LogLevel.Trace, message, exception);

        public static void Debug(this ILogger logger, string message, Exception? exception = null)
            => logger.Log(LogLevel.Debug, message, exception);

        public static void Info(this ILogger logger, string message, Exception? exception = null)
            => logger.Log(LogLevel.Info, message, exception);

        public static void Warn(this ILogger logger, string message, Exception? exception = null)
            => logger.Log(LogLevel.Warning, message, exception);

        public static void Error(this ILogger logger, string message, Exception? exception)
            => logger.Log(LogLevel.Error, message, exception);
        public static void Error(this ILogger logger, Exception exception, string message)
            => logger.Log(LogLevel.Error, message, exception);

        public static void Critical(this ILogger logger, string message, Exception? exception = null)
            => logger.Log(LogLevel.Critical, message, exception);


        public static void CriticalMethod(this ILogger logger, Exception ex, Func<string?> getArgs, Func<string?>? getMethodResult = null, [CallerMemberName] string? methodName = null)
            => Log(logger, LogLevel.Critical, LogMessageMethod(getArgs, getMethodResult, methodName), ex);

        public static void ErrorMethod(this ILogger logger, Exception ex, Func<string?> getArgs, Func<string?>? getMethodResult = null, [CallerMemberName] string? methodName = null)
            => Log(logger, LogLevel.Error, LogMessageMethod(getArgs, getMethodResult, methodName), ex);

        public static void WarningMethod(this ILogger logger, Exception? ex, Func<string?> getArgs, Func<string?>? getMethodResult = null, [CallerMemberName] string? methodName = null)
            => Log(logger, LogLevel.Warning, LogMessageMethod(getArgs, getMethodResult, methodName), ex);

        public static void InfoMethod(this ILogger logger, Func<string?> getArgs, Func<string?>? getMethodResult = null, [CallerMemberName] string? methodName = null)
            => Log(logger, LogLevel.Info, LogMessageMethod(getArgs, getMethodResult, methodName));

        public static void DebugMethod(this ILogger logger, Func<string?> getArgs, Func<string?>? getMethodResult = null, [CallerMemberName] string? methodName = null)
            => Log(logger, LogLevel.Debug, LogMessageMethod(getArgs, getMethodResult, methodName));

        public static void TraceMethod(this ILogger logger, Func<string?> getArgs, Func<string?>? getMethodResult = null, [CallerMemberName] string? methodName = null)
            => Log(logger, LogLevel.Trace, LogMessageMethod(getArgs, getMethodResult, methodName));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(this ILogger logger, LogLevel level, Func<string?> getMessage, Exception? ex = null)
        {
            if (logger.IsEnabled(level))
            {
                logger.Log(level, getMessage(), ex, null);
            }
        }

        /// <summary> make message as $"{methodName}({methodArgs}) {methodResult}" </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<string> LogMessageMethod(Func<string?> methodArgs, Func<string?>? methodResult = null, [CallerMemberName] string? methodName = null)
            => () => $"{methodName}({methodArgs()}){methodResult?.Invoke()}";
    }
}