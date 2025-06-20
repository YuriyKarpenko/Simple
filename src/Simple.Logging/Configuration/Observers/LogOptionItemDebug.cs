namespace Simple.Logging.Configuration;

public class LogOptionItemDebug(ILogOptionItem root) : LogOptionItem(SConfigName, root)
{
    public const string SConfigName = "Debug";
}