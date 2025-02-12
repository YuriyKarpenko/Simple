using System;
using System.Reflection;

using Simple.Logging.Configuration;
using Simple.Logging.Messages;

namespace Simple.Logging.Observers;

public abstract class ObserverBase : ILogObserver
{
    protected readonly ILogOptions _options;
    public ObserverBase(ILogOptions options)
    {
        _options = options;
    }


    public LoggerFilterItem FilterItem => _options.EnsureOptionItem(Name).LogLevel;

    /// <inheritdoc />
    public abstract string Name { get; }

    #region IObserver

    /// <inheritdoc />
    public void OnCompleted() { }

    /// <inheritdoc />
    public void OnError(Exception _) { }

    /// <inheritdoc />
    public void OnNext(LogMessage value)
    {
        if (FilterItem.Filter(value.Level, value.LogSource) || _options.LogLevel.Filter(value.Level, value.LogSource))
        {
            Write(value);
        }
    }

    #endregion

    protected abstract void Write(LogMessage value);
}

public abstract class ObserverBase<T> : ObserverBase where T : ObserverBase
{
    public static readonly string ObserverName;
    static ObserverBase()
    {
        var a = typeof(T).GetCustomAttribute<LoggerNameAttribute>();
        ObserverName = Throw.IsArgumentNullException(a, nameof(LoggerNameAttribute)).Name;
    }


    public ObserverBase(ILogOptions options) : base(options) { }

    public override string Name => ObserverName;
}