using System;

namespace Simple.Logging
{
    public static class LoggerExtensions
    {
        public static void Debug(this ILogger logger, int eventId, Exception exception, string message)
        {
            logger.Log(LogLevel.Debug, eventId, exception, message);
        }

        public static void Debug(this ILogger logger, int eventId, string message)
        {
            logger.Log(LogLevel.Debug, eventId, message);
        }

        public static void Debug(this ILogger logger, Exception exception, string message)
        {
            logger.Log(LogLevel.Debug, exception, message);
        }

        public static void Debug(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Debug, message);
        }

        public static void Trace(this ILogger logger, int eventId, Exception exception, string message)
        {
            logger.Log(LogLevel.Trace, eventId, exception, message);
        }

        public static void Trace(this ILogger logger, int eventId, string message)
        {
            logger.Log(LogLevel.Trace, eventId, message);
        }

        public static void Trace(this ILogger logger, Exception exception, string message)
        {
            logger.Log(LogLevel.Trace, exception, message);
        }

        public static void Trace(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Trace, message);
        }

        public static void Info(this ILogger logger, int eventId, Exception exception, string message)
        {
            logger.Log(LogLevel.Info, eventId, exception, message);
        }

        public static void Info(this ILogger logger, int eventId, string message)
        {
            logger.Log(LogLevel.Info, eventId, message);
        }

        public static void Info(this ILogger logger, Exception exception, string message)
        {
            logger.Log(LogLevel.Info, exception, message);
        }

        public static void Info(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Info, message);
        }

        public static void Warn(this ILogger logger, int eventId, Exception exception, string message)
        {
            logger.Log(LogLevel.Warning, eventId, exception, message);
        }

        public static void Warn(this ILogger logger, int eventId, string message)
        {
            logger.Log(LogLevel.Warning, eventId, message);
        }

        public static void Warn(this ILogger logger, Exception exception, string message)
        {
            logger.Log(LogLevel.Warning, exception, message);
        }

        public static void Warn(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Warning, message);
        }

        public static void Error(this ILogger logger, int eventId, Exception exception, string message)
        {
            logger.Log(LogLevel.Error, eventId, exception, message);
        }

        public static void Error(this ILogger logger, int eventId, string message)
        {
            logger.Log(LogLevel.Error, eventId, message);
        }

        public static void Error(this ILogger logger, Exception exception, string message)
        {
            logger.Log(LogLevel.Error, exception, message);
        }

        public static void Error(this ILogger logger, Exception exception)
        {
            logger.Log(LogLevel.Error, exception, string.Empty);
        }

        public static void Error(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Error, message);
        }


        public static void Critical(this ILogger logger, int eventId, Exception exception, string message)
        {
            logger.Log(LogLevel.Critical, eventId, exception, message);
        }

        public static void Critical(this ILogger logger, Exception exception, string message)
        {
            logger.Log(LogLevel.Critical, exception, message);
        }

        public static void Critical(this ILogger logger, string message)
        {
            logger.Log(LogLevel.Critical, message);
        }

        public static void Log(this ILogger logger, LogLevel logLevel, string message)
        {
            logger.Log(logLevel, 0, null, message);
        }

        public static void Log(this ILogger logger, LogLevel logLevel, int eventId, string message)
        {
            logger.Log(logLevel, eventId, null, message);
        }

        public static void Log(this ILogger logger, LogLevel logLevel, Exception exception, string message)
        {
            logger.Log(logLevel, 0, exception, message);
        }

        public static void Log(this ILogger logger, LogLevel logLevel, int eventId, Exception? exception, string message)
        {
            Throw.IsArgumentNullException(logger, nameof(logger));

            logger.Log(logLevel, eventId, message, exception);
        }

        //public static IDisposable BeginScope(this ILogger logger, string messageFormat)
        //{
        //    Throw.IsArgumentNullException(logger, nameof(logger));

        //    return logger.BeginScope(messageFormat);
        //}
    }
}
