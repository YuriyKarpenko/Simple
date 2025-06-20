using System;
using System.Collections.Generic;

using Level = Microsoft.Extensions.Logging.LogLevel;

namespace Simple.Logging.Configuration;

public class LogOptionItemConsole : LogOptionItem
{
    public const string SConfigName = "Console";

    public LogOptionItemConsole(ILogOptionItem root) : base(SConfigName, root)
    {
        BackColors = new Dictionary<Level, ConsoleColor>
        {
            { Level.Critical, ConsoleColor.Red },
            { Level.Error, ConsoleColor.Red },
            { Level.Warning, ConsoleColor.Black },
            { Level.Information, ConsoleColor.Black },
            { Level.Debug, ConsoleColor.Black },
            { Level.Trace, ConsoleColor.Black },
        };

        ForeColors = new Dictionary<Level, ConsoleColor>
        {
            { Level.Critical, ConsoleColor.White },
            { Level.Error, ConsoleColor.Black },
            { Level.Warning, ConsoleColor.Yellow },
            { Level.Information, ConsoleColor.DarkGreen },
            { Level.Debug, ConsoleColor.Gray },
            { Level.Trace, ConsoleColor.Gray },
        };
    }

    public bool IncludeScopes { get; set; }
    public bool DisableColors { get; set; }
    public IDictionary<Level, ConsoleColor> BackColors { get; set; }
    public IDictionary<Level, ConsoleColor> ForeColors { get; set; }

    public override void ApplyOptions(LogOptionItemRaw rawData)
    {
        base.ApplyOptions(rawData);
        DisableColors = rawData.TryGetValue(nameof(DisableColors), out var sD) && bool.Parse(sD);
        IncludeScopes = rawData.TryGetValue(nameof(IncludeScopes), out var sI) && bool.Parse(sI);
        //  TODO: load colors
    }
}