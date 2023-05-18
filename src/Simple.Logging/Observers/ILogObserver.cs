using System;

namespace Simple.Logging.Observers
{
    public interface ILogObserver
    {
        /// <summary> Observers name for filtering </summary>
        string Name { get; }
    }

    public interface ILogObserver<T> : ILogObserver, IObserver<T> { }
}
