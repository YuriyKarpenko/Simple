using System;
using System.Collections.Concurrent;

using Simple.Logging.Messages;
using Simple.Logging.Observers;

namespace Simple.Logging;

public class LoggerFactory : ILoggerFactory, IDisposable
{
    private readonly ConcurrentDictionary<string, ILogger> _cache = new();

    #region ILoggerFactory

    /// <inheritdoc />
    public ILogger CreateLogger(string logSource)
    {
        return _cache.GetOrAdd(logSource, key => new Logger(key, LogMessageBus.Instance));
    }

    public void AddProvider(ILoggerProvider provider)
    {
        if (provider is ILogObserver lo)
        {
            LogMessageBus.Instance.Subscribe(lo);
            return;
        }

        Throw.Exception(new NotSupportedException("LoggerFactory.AddProvider(ILoggerProvider provider)"));
    }

    #endregion

    /// <inheritdoc />
    public ILogger CreateLogger(Type logSource)
    {
        var t = Throw.IsArgumentNullException(logSource, nameof(logSource));
        return CreateLogger(t.FullName ?? t.Name);
    }

    /// <inheritdoc />
    public ILogger CreateLogger<T>()
        => CreateLogger(typeof(T));

    #region IDisposable

    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~DefaultLoggerFactory()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}