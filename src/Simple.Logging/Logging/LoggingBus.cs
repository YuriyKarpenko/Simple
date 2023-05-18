using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Simple.Logging.Messages;

namespace Simple.Logging.Logging
{
    public class LoggingBus<TMessage> : ILoggingBus<TMessage>
    {
        public static readonly ILoggingBus<TMessage> Instance = new LoggingBus<TMessage>();


        private readonly IList<SubscriberEntry<TMessage>> _targets = new List<SubscriberEntry<TMessage>>();

        public virtual void Push(TMessage entry)
        {
            Task.Run(() =>
            {
                foreach (var t in _targets)
                {
                    t.OnNext(entry);
                }
            }).ConfigureAwait(false);
        }

        public IDisposable Subscribe(IObserver<TMessage> observer)
        {
            var res = new SubscriberEntry<TMessage>(observer, RemoveSubscriber);
            _targets.Add(res);
            return res;
        }

        public void Clear()
        {
            _targets.Clear();
        }

        private void RemoveSubscriber(SubscriberEntry<TMessage> value)
        {
            //Volatile.Write<Node>(ref tables._buckets[bucketNo], new Node(key, value, hashcode, tables._buckets[bucketNo]));
            //checked
            //{
            //    tables._countPerLock[lockNo]++;
            //}
            _targets.Remove(value);
        }

        private class SubscriberEntry<T> : IObserver<T>, IDisposable
        {
            private readonly IObserver<T> _value;
            private readonly Action<SubscriberEntry<T>> _onDispose;

            public SubscriberEntry(IObserver<T> value, Action<SubscriberEntry<T>> onDispose)
            {
                _value = value;
                _onDispose = onDispose;
            }

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
            public void OnNext(T value) => _value.OnNext(value);

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

    public class LoggingBus : LoggingBus<ILogMessage>
    {
    }
}
