using System.Collections.Generic;

namespace Simple.Logging.Configuration
{
    public interface ILogOptions
    {
        /// <summary> [ObserverName, Json] (observers RAW options) </summary>
        IDictionary<string, string> ObserversOptions { get; }

        /// <summary> Filters of all observers </summary>
        LoggerFilterOptions FilterOptions { get; }
    }
}