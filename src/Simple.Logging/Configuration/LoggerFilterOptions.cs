﻿using System;
using System.Collections.Generic;
using System.Linq;

using Simple.Logging.Messages;
using Simple.Logging.Observers;

namespace Simple.Logging.Configuration
{
    public class LoggerFilterOptions
    {
        public static LoggerFilterOptions Instance = new LoggerFilterOptions();

        private LoggerFilterOptions()
        {
            Rules = new Dictionary<string, LoggerFilterItem>(StringComparer.OrdinalIgnoreCase);
            Default = new LoggerFilterItem(null);
        }

        public LoggerFilterItem Default { get; set; }

        /// <summary> [ObserverName, ObserverFilter] Observers filters </summary>
        public IDictionary<string, LoggerFilterItem> Rules { get; }


        public bool FilterIn(ILogMessage message)
        {
            var fullName = message.LogSource;
            var matches = Rules.Values
                .SelectMany(i => i.GetRules(fullName))
                .Union(Default.GetRules(fullName))
                .ToArray();
            var level = Default.FilterMatces(matches);
            return message.Level >= level;
        }

        public bool FilterOut(ILogMessage message, ILogObserver observer)
        {
            return Rules.TryGetValue(observer.Name, out var rules)
                ? rules.Filter(message)
                : Default.Filter(message);
        }
    }
}
