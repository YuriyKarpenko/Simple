using System;
using System.Collections.Generic;

namespace Simple.Logging.Messages;

public class DefaultLogMessageFactory : ILogMessageFactory
{
    public ILogMessage CreateMessage<TState>(string logSource, LogLevel level, TState state, Exception? exception, Func<TState, Exception?, string>? formatter)
    {
        formatter ??= DefaultFormatMessage;
        var msg = formatter(state, exception);
        return new ILogMessage(logSource, level, msg, exception);
    }

    public static string DefaultFormatMessage<TState>(TState? state, Exception? exception)
    {
        var err = exception == null ? null : $"\n\t{exception.Message}";
        return $"{state}{err}";
    }

    public string CreateScopes()
    {
        var list = new List<object?>();
        LogManager.ScopeProvider.ForEachScope((o, l) => l.Add(o), list);
        return CreateScopes(list);
    }

    public string CreateScopes(List<object?> scopes)
        => scopes.Count switch
        {
            0 => string.Empty,
            1 => scopes[0]?.ToString() ?? string.Empty,
            _ => $"[{scopes.AsString(" => ")}]\n\t"
        };

    public string ToStringWithoutLevel(ILogMessage message)
    {
        var idx = message.LogSource.LastIndexOf('.');
        var name = idx > 0 ? message.LogSource.Substring(idx + 1) : message.LogSource;

        return $"- {message.Created,-10:T}: {name,-10} : {message.State}\n";
    }
}