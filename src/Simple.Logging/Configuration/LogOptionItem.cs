using System;
using System.Collections.Generic;

namespace Simple.Logging.Configuration;
public class LogOptionItem
{
    public LogOptionItem(LogLevel minLevel)
    {
        FilterItem = new LoggerFilterItem(null, minLevel);
        Options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public LoggerFilterItem FilterItem { get; set; }

    /// <summary> [OptionName, OptionValue] (observer RAW options) </summary>
    public Dictionary<string, string> Options { get; }
}