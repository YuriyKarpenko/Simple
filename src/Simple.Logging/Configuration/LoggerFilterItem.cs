using System;
using System.Linq;

namespace Simple.Logging.Configuration
{
    /// <summary> Defines a rule used to filter log messages </summary>
    public class LoggerFilterItem : DictionaryString<LogLevel>
    {
        public LoggerFilterItem(LogLevel minLevel = LogLevel.Error)
        {
            Default = minLevel;
        }

        public LogLevel Default { get; set; }


        public override LogLevel this[string nameSpace]
        {
            get => IsDefault(nameSpace) ? Default : _inner[nameSpace];
            set
            {
                if (IsDefault(nameSpace))
                {
                    Default = value;
                }
                else
                {
                    _inner[nameSpace] = value;
                }
            }
        }

        public bool Filter(LogLevel level, string logSource)
        {
            if (level >= Default)
            {
                return true;
            }
            return _inner.Any(i => level >= i.Value && logSource.StartsWith(i.Key, StringComparison.OrdinalIgnoreCase));
        }


        private static bool IsDefault(string nameSpace)
            => string.IsNullOrWhiteSpace(nameSpace) || nameof(Default).Equals(nameSpace, StringComparison.InvariantCultureIgnoreCase);
    }
}