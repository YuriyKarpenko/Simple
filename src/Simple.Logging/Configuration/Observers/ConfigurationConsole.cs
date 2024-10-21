using System;
using System.Collections.Generic;

namespace Simple.Logging.Configuration
{
    public class ConfigurationConsole : ConfigObserverBase
    {
        public ConfigurationConsole()
        {
            BackColors = new Dictionary<LogLevel, ConsoleColor>
            {
                { LogLevel.Critical, ConsoleColor.Red },
                { LogLevel.Error, ConsoleColor.Red },
                { LogLevel.Warning, ConsoleColor.Black },
                { LogLevel.Info, ConsoleColor.Black },
                { LogLevel.Debug, ConsoleColor.Black },
                { LogLevel.Trace, ConsoleColor.Black },
            };

            ForeColors = new Dictionary<LogLevel, ConsoleColor>
            {
                { LogLevel.Critical, ConsoleColor.White },
                { LogLevel.Error, ConsoleColor.Black },
                { LogLevel.Warning, ConsoleColor.Yellow },
                { LogLevel.Info, ConsoleColor.DarkGreen },
                { LogLevel.Debug, ConsoleColor.Gray },
                { LogLevel.Trace, ConsoleColor.Gray },
            };
        }

        public bool IncludeScopes { get; set; }
        public bool DisableColors { get; set; }
        public IDictionary<LogLevel, ConsoleColor> BackColors { get; set; }
        public IDictionary<LogLevel, ConsoleColor> ForeColors { get; set; }


        public override void ApplyOptions(IDictionary<string, string> options)
        {
            DisableColors = options.TryGetValue(nameof(DisableColors), out var sD) && bool.Parse(sD);
            IncludeScopes = options.TryGetValue(nameof(IncludeScopes), out var sI) && bool.Parse(sI);
        }
    }
}