namespace Simple.Logging.Configuration;

public class LogOptionItem
{
    public LogOptionItem(LogLevel minLevel)
    {
        LogLevel = new LoggerFilterItem(minLevel);
        Options = new DictionaryString<string>();
    }

    public LoggerFilterItem LogLevel { get; set; }

    /// <summary> [OptionName, OptionValue] (observer RAW options) </summary>
    public DictionaryString<string> Options { get; }
}