using System;
using System.Collections.Generic;
using System.Linq;

using Simple.Logging.Messages;

namespace Simple.Logging.Configuration
{
    /// <summary>
    /// Defines a rule used to filter log messages
    /// </summary>
    public class LoggerFilterItem
    {
        public static LoggerFilterItem Default = new LoggerFilterItem(null);

        private readonly IDictionary<string, LogLevel> _rules = new Dictionary<string, LogLevel>(StringComparer.OrdinalIgnoreCase);


        public LoggerFilterItem(IDictionary<string, LogLevel>? rules)
        {
            if (rules?.Any() == true)
            {
                _rules = rules;

                MinLevel = _rules.TryGetValue(string.Empty, out var level)
                    ? level
                    : _rules.Values.Min();
            }
        }

        public LogLevel MinLevel { get; set; }


        public Match[] GetRules(string fullName)
        {
            var values = _rules
                .Where(i => fullName.StartsWith(i.Key, StringComparison.OrdinalIgnoreCase))
                .Select(i => new Match(i.Key, i.Value))
                .ToArray();

            return values;
        }

        public bool Filter(ILogMessage message)
        {
            var fullName = message.LogSource.FullName;
            var matches = GetRules(fullName);
            var level = FilterMatces(matches);
            return message.Level >= level;
        }

        public LogLevel FilterMatces(Match[] matches)
        {
            return matches.Count() switch
            {
                0 => MinLevel,
                1 => matches[0].Level,
                _ => matches.Aggregate((v, a) => v.Length > a.Length ? v : a).Level
            };
        }


        public class Match //: IComparable<Match>
        {
            public Match(string key, LogLevel level)
            {
                Length = key.Length;
                Level = level;
            }

            public int Length { get; }
            public LogLevel Level { get; }
        }
    }
}
