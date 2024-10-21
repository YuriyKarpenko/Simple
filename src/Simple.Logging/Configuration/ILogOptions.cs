namespace Simple.Logging.Configuration
{
    public interface ILogOptions
    {
        /// <summary> Common filter </summary>
        LoggerFilterItem LogLevel { get; }

        /// <summary> Can process the log-message with <paramref name="level"/> and <paramref name="logSource"/> /> </summary>
        /// <returns>Can we process this message</returns>
        bool FilterIn(LogLevel level, string logSource);

        LogOptionItem EnsureOptionItem(string observerName);

        //void SetFilterItem(string observerName, LoggerFilterItem filterItem);
    }
}