using System;

namespace Simple.Logging
{
    public static class LoggerExtensions
    {
        public static void Trace(this ILogger logger, string message, Exception? exception = null)
        {
            logger.Log(LogLevel.Trace, message, exception);
        }

        public static void Debug(this ILogger logger, string message, Exception? exception = null)
        {
            logger.Log(LogLevel.Debug, message, exception);
        }

        public static void Info(this ILogger logger, string message, Exception? exception = null)
        {
            logger.Log(LogLevel.Info, message, exception);
        }

        public static void Warn(this ILogger logger, string message, Exception? exception = null)
        {
            logger.Log(LogLevel.Warning, message, exception);
        }

        public static void Error(this ILogger logger, string message, Exception? exception)
        {
            logger.Log(LogLevel.Error, message, exception);
        }

        public static void Critical(this ILogger logger, string message, Exception? exception = null)
        {
            logger.Log(LogLevel.Critical, message, exception);
        }
    }
}