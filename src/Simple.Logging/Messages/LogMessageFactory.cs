using System;
using System.Collections.Generic;

namespace Simple.Logging.Messages;

public interface ILogMessageFactory
{
    LogMessage CreateMessage<TState>(string logSource, LogLevel level, TState state, Exception? exception, Func<TState, Exception?, string>? formatter = null);
    string CreateScopes();
    string ToStringWithoutLevel(LogMessage message, bool includeScopes);
}

public class LogMessageFactory : ILogMessageFactory
{
    public LogMessage CreateMessage<TState>(string logSource, LogLevel level, TState state, Exception? exception, Func<TState, Exception?, string>? formatter)
    {
        formatter ??= DefaultFormatMessage;
        var msg = formatter(state, exception);
        return new LogMessage(logSource, level, msg, exception);
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

    private static string CreateScopes(List<object?> scopes)
        => scopes.Count switch
        {
            0 => string.Empty,
            1 => $"[{scopes[0]}] : ",
            _ => $"[{scopes.AsString(" => ")}]\n\t"
        };

    public string ToStringWithoutLevel(LogMessage message, bool includeScopes)
    {
        var idx = message.LogSource.LastIndexOf('.');
        var name = idx > 0 ? message.LogSource.Substring(idx + 1) : message.LogSource;
        var scopes = includeScopes ? CreateScopes() : string.Empty;

        return $"- {message.Created,-9:T}: {scopes}{name,-10} : {message.State}\n";
    }
}