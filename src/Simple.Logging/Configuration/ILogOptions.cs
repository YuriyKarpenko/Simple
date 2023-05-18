using System.Collections.Generic;

namespace Simple.Logging.Configuration
{
    public interface ILogOptions
    {
        LogLevel MinLevel { get; }
        /// <summary> [ObserverName, Json] </summary>
        IDictionary<string, string> ObserversOptions { get; }
        LoggerFilterOptions FilterOptions { get; }
    }
}
