using System;
using System.Collections.Generic;
using System.Linq;

using Simple.Logging.Messages;

namespace Simple.Logging.Configuration
{
    /// <summary> Filters of all observers </summary>
    public class LoggerFilterOptions
    {
        /// <summary> [ObserverName, ObserverFilter] Observers filters </summary>
        private readonly IDictionary<string, LoggerFilterItem> _obseversFilterItems;
        public LoggerFilterOptions()
        {
            _obseversFilterItems = new Dictionary<string, LoggerFilterItem>(StringComparer.OrdinalIgnoreCase);
            Default = new LoggerFilterItem(null);
        }

        public LoggerFilterItem Default { get; set; }

        public bool FilterIn(LogLevel level, string logSource)
        {
            return Default.Filter(level, logSource) ||
                _obseversFilterItems.Values.Any(filter => filter.Filter(level, logSource));
        }

        public LoggerFilterItem EnsureFilterItem(string observerName)
        {
            if (!_obseversFilterItems.TryGetValue(observerName, out var filterItem))
            {
                _obseversFilterItems[observerName] = filterItem = new LoggerFilterItem(null);
                filterItem.MinLevel = Default.MinLevel;
            }
            return filterItem;
        }

        public void SetFilterItem(string observerName, LoggerFilterItem filterItem)
        {
            Throw.IsArgumentNullException(observerName, nameof(observerName));
            Throw.IsArgumentNullException(filterItem, nameof(filterItem));
            _obseversFilterItems[observerName] = filterItem;
        }
    }
}