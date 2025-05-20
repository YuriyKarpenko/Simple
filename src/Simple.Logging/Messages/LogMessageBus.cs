using System;
using System.Collections.Concurrent;

using Simple.Logging.Observers;

namespace Simple.Logging.Messages;

public interface ILogMessageBus<TLogMessage> : IObservable<TLogMessage>
{
    /// <summary> Eter new message for observers </summary>
    void Push(TLogMessage entry);
}

public class LogMessageBus : ILogMessageBus<LogMessage>
{
    public static readonly ILogMessageBus<LogMessage> Instance = new LogMessageBus();


    private readonly ConcurrentDictionary<string, SubscriberEntry> _targets
        = new ConcurrentDictionary<string, SubscriberEntry>(StringComparer.OrdinalIgnoreCase);

    public virtual void Push(LogMessage entry)
    {
        //Task.Run(() =>
        {
            var targets = _targets.Values;
            foreach (var t in targets)
            {
                t.OnNext(entry);
            }
        }
        //).ConfigureAwait(false);
    }

    public IDisposable Subscribe(IObserver<LogMessage> observer)
    {
        if (observer is ILogObserver lo)
        {
            if (_targets.TryGetValue(lo.Name, out var entry))
            {
                entry.Dispose();
            }

            var res = new SubscriberEntry(lo, RemoveSubscriber);
            _targets[lo.Name] = res;
            return res;
        }

        return NoopDisposable.Instance;
    }

    //public void Clear()
    //{
    //    _targets.Clear();
    //}

    private void RemoveSubscriber(SubscriberEntry value)
    {
        //Volatile.Write<Node>(ref tables._buckets[bucketNo], new Node(key, value, hashcode, tables._buckets[bucketNo]));
        //checked
        //{
        //    tables._countPerLock[lockNo]++;
        //}
        if (_targets.TryRemove(value.Name, out var d))
        {
            d.Dispose();
        }
    }

    private class SubscriberEntry : IObserver<LogMessage>, IDisposable
    {
        private readonly IObserver<LogMessage> _value;
        private readonly Action<SubscriberEntry> _onDispose;

        public SubscriberEntry(ILogObserver value, Action<SubscriberEntry> onDispose)
        {
            _value = value;
            _onDispose = onDispose;
            Name = value.Name;
        }


        public string Name { get; }

        #region IDisposable

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _onDispose(this);
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IObserver

        public void OnCompleted() => _value.OnCompleted();
        public void OnError(Exception error) => _value.OnError(error);
        public void OnNext(LogMessage value) => _value.OnNext(value);

        #endregion
    }

    private class NoopDisposable : IDisposable
    {
        public static NoopDisposable Instance = new NoopDisposable();

        public void Dispose()
        {
        }
    }
}