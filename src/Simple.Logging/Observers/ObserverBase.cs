using System;
using System.Reflection;

using Simple.Logging.Configuration;
using Simple.Logging.Messages;

namespace Simple.Logging.Observers;

public interface ILogObserver : IObserver<LogMessage>
{
    /// <summary> Observers name for filtering </summary>
    string Name { get; }
    //public LoggerFilterItem FilterItem { get; }
}

public abstract class ObserverBase : ILogObserver
{
    protected abstract ILogOptionItem OptionItem { get; }


    /// <inheritdoc />
    public virtual string Name => OptionItem.ConfigName;

    #region IObserver

    /// <inheritdoc />
    public void OnCompleted() { }

    /// <inheritdoc />
    public void OnError(Exception _) { }

    /// <inheritdoc />
    public void OnNext(LogMessage value)
    {
        //if (FilterItem.Filter(value.Level, value.LogSource) || _options.LogLevel.Filter(value.Level, value.LogSource))
        if (OptionItem.LogLevel.Filter(value.Level, value.LogSource))
        {
            Write(value);
        }
    }

    #endregion

    protected abstract void Write(LogMessage value);
}

public abstract class ObserverBase<T> : ObserverBase where T : ObserverBase
{
    public static readonly string ConfigName;
    static ObserverBase()
    {
        var a = typeof(T).GetCustomAttribute<LoggerNameAttribute>();
        ConfigName = Throw.IsArgumentNullException(a, nameof(LoggerNameAttribute)).Name;
    }


    public override string Name => ConfigName;
}