using System;

namespace Simple.Logging.Messages
{
    public class DefaultLogMessageFactory : ILogMessageFactory
    {
        public ILogMessage CreateMessage<TState>(string logSource, LogLevel level, TState state, Exception? exception, Func<TState, Exception?, string>? formatter = null)
        {
            formatter ??= DefaultFormatMessage;
            var msg = formatter(state, exception);
            return new LogMessageString(logSource, level, msg, exception);
        }

        public static string DefaultFormatMessage<TState>(TState? state, Exception? exception)
        {
            var err = exception == null ? null : $"\n\t{exception.Message}";
            return $"{state}{err}";
        }

        public string CreateScopes(object[] scopes)
            => scopes.Length > 0
                ? $"[{scopes.AsString(" => ")}]\n\t"
                : string.Empty;

        public string ToStringWithoutLevel(ILogMessage message)
        {
            var idx = message.LogSource.LastIndexOf('.');
            var name = idx > 0 ? message.LogSource.Substring(idx + 1) : message.LogSource;

            return $"- {message.Created,-10:T}: {name,-10} : {message.State}\n";
        }
    }
}