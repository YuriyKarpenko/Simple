using System;
using System.Reflection;

using Simple.Helpers;
using Simple.Logging.Messages;

namespace Simple.Logging.Observers
{
    public abstract class ObserverBase : ILogObserver<ILogMessage>
    {
        /// <inheritdoc />
        public abstract string Name { get; }

        protected abstract void Write(ILogMessage value);

        /// <inheritdoc />
        public void OnCompleted() { }

        /// <inheritdoc />
        public void OnError(Exception _) { }

        /// <inheritdoc />
        public void OnNext(ILogMessage value)
        {
            if (LogManager.FilterOut(value, this))
            {
                Write(value);
            }
        }
    }

    public abstract class ObserverBase<T> : ObserverBase where T : ObserverBase
    {
        public static string ObserverName => Throw.IsArgumentNullException(typeof(T).GetCustomAttribute<LoggerNameAttribute>(), nameof(LoggerNameAttribute)).Name;

        public override string Name => ObserverName;
    }
}