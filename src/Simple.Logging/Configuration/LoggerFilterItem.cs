using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Logging.Configuration
{
    /// <summary> Defines a rule used to filter log messages </summary>
    public class LoggerFilterItem
    {
        private readonly IDictionary<string, LogLevel> _rules;
        public LoggerFilterItem(IDictionary<string, LogLevel>? rules, LogLevel minLevel = LogLevel.Error)
        {
            _rules = rules?.Any() == true
                ? new Dictionary<string, LogLevel>(rules, StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, LogLevel>(StringComparer.OrdinalIgnoreCase);

            MinLevel = _rules.TryGetValue(string.Empty, out var level)
                ? level
                : minLevel;
        }

        public LogLevel MinLevel { get; set; }


        public LoggerFilterItem AddRule(string nameSpace, LogLevel level)
        {
            if (string.IsNullOrWhiteSpace(nameSpace))
            {
                MinLevel = level;
            }
            else
            {
                _rules[nameSpace] = level;
            }
            return this;
        }

        public bool Filter(LogLevel level, string logSource)
        {
            if (level >= MinLevel)
            {
                return true;
            }
            return _rules.Any(i => level >= i.Value && logSource.StartsWith(i.Key, StringComparison.OrdinalIgnoreCase));
        }
    }
}