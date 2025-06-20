namespace Simple.Logging.Configuration;

public interface ILogOptionItem
{
    string ConfigName { get; }

    /// <summary> filter item </summary>
    LoggerFilterItem LogLevel { get; }
}

public class LogOptionItem(string configName, ILogOptionItem? root) : ILogOptionItem
{
    public string ConfigName => configName;
    public LoggerFilterItem LogLevel { get; } = CloneFilter(root?.LogLevel);

    /// <summary> Init external observer with RAW options </summary>
    /// <param name="rawData"> [OptionName, OptionValue] (observers RAW options) </param>
    public virtual void ApplyOptions(LogOptionItemRaw rawData)
    {
        LogLevel.Merge(rawData.LogLevel);
    }


    private static LoggerFilterItem CloneFilter(LoggerFilterItem? src)
    {
        var res = new LoggerFilterItem();
        if (src != null)
        {
            res.Merge(src);
            res.Default = src.Default;
        }
        return res;
    }
}

public class LogOptionItemRaw(LogLevel defaultLevel) : DicString<string>, ILogOptionItem
{
    public const string SConfigName = "RAW";

    public string ConfigName => SConfigName;
    public LoggerFilterItem LogLevel { get; } = new LoggerFilterItem(defaultLevel);
}