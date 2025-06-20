using System;
using System.Linq;

namespace Simple.Logging.Configuration;

/// <summary> Defines a rule used to filter log messages </summary>
public class LoggerFilterItem(LogLevel minLevel = LogLevel.Error) : DicString<LogLevel>(StringComparer.OrdinalIgnoreCase)
{
    public LogLevel Default { get; set; } = minLevel;


    public new LogLevel this[string nameSpace]
    {
        get => IsDefault(nameSpace) ? Default : base[nameSpace];
        set
        {
            if (IsDefault(nameSpace))
            {
                Default = value;
            }
            else
            {
                base[nameSpace] = value;
            }
        }
    }

    public bool Filter(LogLevel level, string logSource)
    {
        var b = level >= Default || this.Any(i => level >= i.Value && logSource.StartsWith(i.Key, StringComparison.OrdinalIgnoreCase));
        return b;
    }


    private static bool IsDefault(string nameSpace)
        => string.IsNullOrWhiteSpace(nameSpace) || nameof(Default).Equals(nameSpace, StringComparison.InvariantCultureIgnoreCase);
}