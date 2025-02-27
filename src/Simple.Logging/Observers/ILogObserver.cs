using System;

using Simple.Logging.Configuration;
using Simple.Logging.Messages;

namespace Simple.Logging.Observers
{
    public interface ILogObserver : IObserver<LogMessage>
    {
        /// <summary> Observers name for filtering </summary>
        string Name { get; }
        public LoggerFilterItem FilterItem { get; }
    }
}