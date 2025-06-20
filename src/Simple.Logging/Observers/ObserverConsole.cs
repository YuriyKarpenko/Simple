using System;
using System.IO;

using Simple.Logging.Configuration;
using Simple.Logging.Messages;

namespace Simple.Logging.Observers;

[LoggerName("Console")]
public class ObserverConsole : ObserverBase//<ObserverConsole>
{
    private readonly LogOptionItemConsole _oi;
    public ObserverConsole(ILogOptions options, Action<LogOptionItemConsole>? confogure = null)
    {
        _oi = new LogOptionItemConsole(options);
        confogure?.Invoke(_oi);
        options.EnsureOptionItem(_oi);
    }

    protected override ILogOptionItem OptionItem => _oi;

    protected override void Write(LogMessage e)
    {
        lock (this)
        {
            var text = LogManager.MessageFactory.ToStringWithoutLevel(e, _oi.IncludeScopes);

            WriteLevel(Console.Out, e.Level);
            Console.Out.WriteLine(text);
        }
    }

    private void WriteLevel(TextWriter tw, LogLevel level)
    {
        var value = $"{level.ToShortName(),-7}";
        if (_oi.DisableColors)
        {
            tw.Write(value);
        }
        else
        {
            try
            {
                if (_oi.BackColors.TryGetValue(level, out var back))
                {
                    Console.BackgroundColor = back;
                }
                if (_oi.ForeColors.TryGetValue(level, out var fore))
                {
                    Console.ForegroundColor = fore;
                }

                tw.Write(value);
            }
            finally
            {
                Console.ResetColor();
            }
        }
    }

    internal const string DefaultForegroundColor = "\x1B[39m\x1B[22m";  // reset to default foreground color
    internal const string DefaultBackgroundColor = "\x1B[49m";          // reset to the background color

    internal static string GetForegroundColorEscapeCode(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black => "\x1B[30m",
            ConsoleColor.DarkRed => "\x1B[31m",
            ConsoleColor.DarkGreen => "\x1B[32m",
            ConsoleColor.DarkYellow => "\x1B[33m",
            ConsoleColor.DarkBlue => "\x1B[34m",
            ConsoleColor.DarkMagenta => "\x1B[35m",
            ConsoleColor.DarkCyan => "\x1B[36m",
            ConsoleColor.Gray => "\x1B[37m",
            ConsoleColor.Red => "\x1B[1m\x1B[31m",
            ConsoleColor.Green => "\x1B[1m\x1B[32m",
            ConsoleColor.Yellow => "\x1B[1m\x1B[33m",
            ConsoleColor.Blue => "\x1B[1m\x1B[34m",
            ConsoleColor.Magenta => "\x1B[1m\x1B[35m",
            ConsoleColor.Cyan => "\x1B[1m\x1B[36m",
            ConsoleColor.White => "\x1B[1m\x1B[37m",
            _ => DefaultForegroundColor // default foreground color
        };
    }

    internal static string GetBackgroundColorEscapeCode(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black => "\x1B[40m",
            ConsoleColor.DarkRed => "\x1B[41m",
            ConsoleColor.DarkGreen => "\x1B[42m",
            ConsoleColor.DarkYellow => "\x1B[43m",
            ConsoleColor.DarkBlue => "\x1B[44m",
            ConsoleColor.DarkMagenta => "\x1B[45m",
            ConsoleColor.DarkCyan => "\x1B[46m",
            ConsoleColor.Gray => "\x1B[47m",
            _ => DefaultBackgroundColor // Use default background color
        };
    }
}

#if !true
public readonly struct EventId
{
    public static implicit operator EventId(int i)
    {
        return new EventId(i);
    }

    public static bool operator ==(EventId left, EventId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(EventId left, EventId right)
    {
        return !left.Equals(right);
    }

    public EventId(int id, string name = null)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; }
    public string Name { get; }

    public override string ToString()
    {
        return Name ?? Id.ToString();
    }

    public bool Equals(EventId other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        return obj is EventId eventId && Equals(eventId);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}

public struct LogMessageEntry
{
    public string LevelString;
    public ConsoleColor? LevelBackground;
    public ConsoleColor? LevelForeground;
    public ConsoleColor? MessageColor;
    public string Message;
}

/// <summary>
/// Represents a type that can create instances of <see cref="ILogger"/>.
/// </summary>
public interface ILoggerProvider : IDisposable
{
    /// <summary>
    /// Creates a new <see cref="ILogger"/> instance.
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns></returns>
    ILogger CreateLogger(string categoryName);
}

/// <summary>
/// Represents a <see cref="ILoggerProvider"/> that is able to consume external scope information.
/// </summary>
public interface ISupportExternalScope
{
    /// <summary>
    /// Sets external scope information source for logger provider.
    /// </summary>
    /// <param name="scopeProvider"></param>
    void SetScopeProvider(IExternalScopeProvider scopeProvider);
}

/// <summary>
/// Represents a storage of common scope data.
/// </summary>
public interface IExternalScopeProvider
{
    /// <summary>
    /// Executes callback for each currently active scope objects in order of creation.
    /// All callbacks are guaranteed to be called inline from this method.
    /// </summary>
    /// <param name="callback">The callback to be executed for every scope object</param>
    /// <param name="state">The state object to be passed into the callback</param>
    /// <typeparam name="TState"></typeparam>
    void ForEachScope<TState>(Action<object, TState> callback, TState state);

    /// <summary>
    /// Adds scope object to the list
    /// </summary>
    /// <param name="state">The scope object</param>
    /// <returns>The <see cref="IDisposable"/> token that removes scope on dispose.</returns>
    IDisposable Push(object state);
}

public class ConsoleLoggerOptions
{
    public bool IncludeScopes { get; set; }
    public bool DisableColors { get; set; }
}

[ProviderAlias("Console")]
public class ConsoleLoggerProvider : ILoggerProvider, ISupportExternalScope
{
    private readonly ConcurrentDictionary<string, ConsoleLogger> _loggers = new ConcurrentDictionary<string, ConsoleLogger>();

    private readonly Func<string, LogLevel, bool> _filter;
    private IConsoleLoggerSettings _settings;
    private readonly ConsoleLoggerProcessor _messageQueue = new ConsoleLoggerProcessor();

    private static readonly Func<string, LogLevel, bool> trueFilter = (cat, level) => true;
    private static readonly Func<string, LogLevel, bool> falseFilter = (cat, level) => false;
    private IDisposable _optionsReloadToken;
    private bool _includeScopes;
    private bool _disableColors;
    private IExternalScopeProvider _scopeProvider;

    [Obsolete("This method is obsolete and will be removed in a future version. The recommended alternative is using LoggerFactory to configure filtering and ConsoleLoggerOptions to configure logging options.")]
    public ConsoleLoggerProvider(Func<string, LogLevel, bool> filter, bool includeScopes)
        : this(filter, includeScopes, false)
    {
    }

    [Obsolete("This method is obsolete and will be removed in a future version. The recommended alternative is using LoggerFactory to configure filtering and ConsoleLoggerOptions to configure logging options.")]
    public ConsoleLoggerProvider(Func<string, LogLevel, bool> filter, bool includeScopes, bool disableColors)
    {
        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        _filter = filter;
        _includeScopes = includeScopes;
        _disableColors = disableColors;
    }

    public ConsoleLoggerProvider(IOptionsMonitor<ConsoleLoggerOptions> options)
    {
        // Filter would be applied on LoggerFactory level
        _filter = trueFilter;
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
        ReloadLoggerOptions(options.CurrentValue);
    }

    private void ReloadLoggerOptions(ConsoleLoggerOptions options)
    {
        _includeScopes = options.IncludeScopes;
        _disableColors = options.DisableColors;
        var scopeProvider = GetScopeProvider();
        foreach (var logger in _loggers.Values)
        {
            logger.ScopeProvider = scopeProvider;
            logger.DisableColors = options.DisableColors;
        }
    }

    [Obsolete("This method is obsolete and will be removed in a future version. The recommended alternative is using LoggerFactory to configure filtering and ConsoleLoggerOptions to configure logging options")]
    public ConsoleLoggerProvider(IConsoleLoggerSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        _settings = settings;

        if (_settings.ChangeToken != null)
        {
            _settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
        }
    }

    private void OnConfigurationReload(object state)
    {
        try
        {
            // The settings object needs to change here, because the old one is probably holding on
            // to an old change token.
            _settings = _settings.Reload();

            _includeScopes = _settings?.IncludeScopes ?? false;

            var scopeProvider = GetScopeProvider();
            foreach (var logger in _loggers.Values)
            {
                logger.Filter = GetFilter(logger.Name, _settings);
                logger.ScopeProvider = scopeProvider;
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error while loading configuration changes.{Environment.NewLine}{ex}");
        }
        finally
        {
            // The token will change each time it reloads, so we need to register again.
            if (_settings?.ChangeToken != null)
            {
                _settings.ChangeToken.RegisterChangeCallback(OnConfigurationReload, null);
            }
        }
    }

    public ILogger CreateLogger(string name)
    {
        return _loggers.GetOrAdd(name, CreateLoggerImplementation);
    }

    private ConsoleLogger CreateLoggerImplementation(string name)
    {
        var includeScopes = _settings?.IncludeScopes ?? _includeScopes;
        var disableColors = _disableColors;

        return new ConsoleLogger(name, GetFilter(name, _settings), includeScopes ? _scopeProvider : null, _messageQueue)
        {
            DisableColors = disableColors
        };
    }

    private Func<string, LogLevel, bool> GetFilter(string name, IConsoleLoggerSettings settings)
    {
        if (_filter != null)
        {
            return _filter;
        }

        if (settings != null)
        {
            foreach (var prefix in GetKeyPrefixes(name))
            {
                LogLevel level;
                if (settings.TryGetSwitch(prefix, out level))
                {
                    return (n, l) => l >= level;
                }
            }
        }

        return falseFilter;
    }

    private IEnumerable<string> GetKeyPrefixes(string name)
    {
        while (!string.IsNullOrEmpty(name))
        {
            yield return name;
            var lastIndexOfDot = name.LastIndexOf('.');
            if (lastIndexOfDot == -1)
            {
                yield return "Default";
                break;
            }
            name = name.Substring(0, lastIndexOfDot);
        }
    }

    private IExternalScopeProvider GetScopeProvider()
    {
        if (_includeScopes && _scopeProvider == null)
        {
            _scopeProvider = new LoggerExternalScopeProvider();
        }
        return _includeScopes ? _scopeProvider : null;
    }

    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
        _messageQueue.Dispose();
    }

    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }
}

[Obsolete("This type is obsolete and will be removed in a future version. The recommended alternative is ConsoleLoggerOptions.")]
public interface IConsoleLoggerSettings
{
    bool IncludeScopes { get; }

    IChangeToken ChangeToken { get; }

    bool TryGetSwitch(string name, out LogLevel level);

    IConsoleLoggerSettings Reload();
}

/// <summary>
/// Propagates notifications that a change has occurred.
/// </summary>
public interface IChangeToken
{
    /// <summary>
    /// Gets a value that indicates if a change has occurred.
    /// </summary>
    bool HasChanged { get; }

    /// <summary>
    /// Indicates if this token will pro-actively raise callbacks. If <c>false</c>, the token consumer must
    /// poll <see cref="HasChanged" /> to detect changes.
    /// </summary>
    bool ActiveChangeCallbacks { get; }

    /// <summary>
    /// Registers for a callback that will be invoked when the entry has changed.
    /// <see cref="HasChanged"/> MUST be set before the callback is invoked.
    /// </summary>
    /// <param name="callback">The <see cref="Action{Object}"/> to invoke.</param>
    /// <param name="state">State to be passed into the callback.</param>
    /// <returns>An <see cref="IDisposable"/> that is used to unregister the callback.</returns>
    IDisposable RegisterChangeCallback(Action<object> callback, object state);
}

[Obsolete("This type is obsolete and will be removed in a future version. The recommended alternative is ConsoleLoggerProvider.")]
public class ConsoleLogger : ILogger
{
    private static readonly string _loglevelPadding = ": ";
    private static readonly string _messagePadding;
    private static readonly string _newLineWithMessagePadding;

    // ConsoleColor does not have a value to specify the 'Default' color
    private readonly ConsoleColor? DefaultConsoleColor = null;

    private readonly ConsoleLoggerProcessor _queueProcessor;
    private Func<string, LogLevel, bool> _filter;

    [ThreadStatic]
    private static StringBuilder _logBuilder;

    static ConsoleLogger()
    {
        var logLevelString = GetLogLevelString(LogLevel.Info);
        _messagePadding = new string(' ', logLevelString.Length + _loglevelPadding.Length);
        _newLineWithMessagePadding = Environment.NewLine + _messagePadding;
    }

    public ConsoleLogger(string name, Func<string, LogLevel, bool> filter, bool includeScopes)
        : this(name, filter, includeScopes ? new LoggerExternalScopeProvider() : null, new ConsoleLoggerProcessor())
    {
    }

    public ConsoleLogger(string name, Func<string, LogLevel, bool> filter, IExternalScopeProvider scopeProvider)
        : this(name, filter, scopeProvider, new ConsoleLoggerProcessor())
    {
    }

    internal ConsoleLogger(string name, Func<string, LogLevel, bool> filter, IExternalScopeProvider scopeProvider, ConsoleLoggerProcessor loggerProcessor)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        Name = name;
        Filter = filter ?? ((category, logLevel) => true);
        ScopeProvider = scopeProvider;
        _queueProcessor = loggerProcessor;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console = new WindowsLogConsole();
        }
        else
        {
            Console = new AnsiLogConsole(new AnsiSystemConsole());
        }
    }

    public IConsole Console
    {
        get { return _queueProcessor.Console; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _queueProcessor.Console = value;
        }
    }

    public Func<string, LogLevel, bool> Filter
    {
        get { return _filter; }
        set
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            _filter = value;
        }
    }

    public string Name { get; }

    [Obsolete("Changing this property has no effect. Use " + nameof(ConsoleLoggerOptions) + "." + nameof(ConsoleLoggerOptions.IncludeScopes) + " instead")]
    public bool IncludeScopes { get; set; }

    internal IExternalScopeProvider ScopeProvider { get; set; }

    public bool DisableColors { get; set; }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        var message = formatter(state, exception);

        if (!string.IsNullOrEmpty(message) || exception != null)
        {
            WriteMessage(logLevel, Name, eventId.Id, message, exception);
        }
    }

    public virtual void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception exception)
    {
        var logBuilder = _logBuilder;
        _logBuilder = null;

        if (logBuilder == null)
        {
            logBuilder = new StringBuilder();
        }

        var logLevelColors = default(ConsoleColors);
        var logLevelString = string.Empty;

        // Example:
        // INFO: ConsoleApp.Program[10]
        //       Request received

        logLevelColors = GetLogLevelConsoleColors(logLevel);
        logLevelString = GetLogLevelString(logLevel);
        // category and event id
        logBuilder.Append(_loglevelPadding);
        logBuilder.Append(logName);
        logBuilder.Append("[");
        logBuilder.Append(eventId);
        logBuilder.AppendLine("]");

        // scope information
        GetScopeInformation(logBuilder);

        if (!string.IsNullOrEmpty(message))
        {
            // message
            logBuilder.Append(_messagePadding);

            var len = logBuilder.Length;
            logBuilder.AppendLine(message);
            logBuilder.Replace(Environment.NewLine, _newLineWithMessagePadding, len, message.Length);
        }

        // Example:
        // System.InvalidOperationException
        //    at Namespace.Class.Function() in File:line X
        if (exception != null)
        {
            // exception message
            logBuilder.AppendLine(exception.ToString());
        }

        var hasLevel = !string.IsNullOrEmpty(logLevelString);
        // Queue log message
        _queueProcessor.EnqueueMessage(new LogMessageEntry()
        {
            Message = logBuilder.ToString(),
            MessageColor = DefaultConsoleColor,
            LevelString = hasLevel ? logLevelString : null,
            LevelBackground = hasLevel ? logLevelColors.Background : null,
            LevelForeground = hasLevel ? logLevelColors.Foreground : null
        });

        logBuilder.Clear();
        if (logBuilder.Capacity > 1024)
        {
            logBuilder.Capacity = 1024;
        }
        _logBuilder = logBuilder;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        if (logLevel == LogLevel.None)
        {
            return false;
        }

        return Filter(Name, logLevel);
    }

    public IDisposable BeginScope<TState>(TState state) => ScopeProvider?.Push(state) ?? NullScope.Instance;

    private static string GetLogLevelString(LogLevel logLevel)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
                return "trce";
            case LogLevel.Debug:
                return "dbug";
            case LogLevel.Info:
                return "info";
            case LogLevel.Warning:
                return "warn";
            case LogLevel.Error:
                return "fail";
            case LogLevel.Critical:
                return "crit";
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel));
        }
    }

    private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel)
    {
        if (DisableColors)
        {
            return new ConsoleColors(null, null);
        }

        // We must explicitly set the background color if we are setting the foreground color,
        // since just setting one can look bad on the users console.
        switch (logLevel)
        {
            case LogLevel.Critical:
                return new ConsoleColors(ConsoleColor.White, ConsoleColor.Red);
            case LogLevel.Error:
                return new ConsoleColors(ConsoleColor.Black, ConsoleColor.Red);
            case LogLevel.Warning:
                return new ConsoleColors(ConsoleColor.Yellow, ConsoleColor.Black);
            case LogLevel.Info:
                return new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black);
            case LogLevel.Debug:
                return new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black);
            case LogLevel.Trace:
                return new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black);
            default:
                return new ConsoleColors(DefaultConsoleColor, DefaultConsoleColor);
        }
    }

    private void GetScopeInformation(StringBuilder stringBuilder)
    {
        var scopeProvider = ScopeProvider;
        if (scopeProvider != null)
        {
            var initialLength = stringBuilder.Length;

            scopeProvider.ForEachScope((scope, state) =>
            {
                var (builder, length) = state;
                var first = length == builder.Length;
                builder.Append(first ? "=> " : " => ").Append(scope);
            }, (stringBuilder, initialLength));

            if (stringBuilder.Length > initialLength)
            {
                stringBuilder.Insert(initialLength, _messagePadding);
                stringBuilder.AppendLine();
            }
        }
    }

    private readonly struct ConsoleColors
    {
        public ConsoleColors(ConsoleColor? foreground, ConsoleColor? background)
        {
            Foreground = foreground;
            Background = background;
        }

        public ConsoleColor? Foreground { get; }

        public ConsoleColor? Background { get; }
    }

    private class AnsiSystemConsole : IAnsiSystemConsole
    {
        public void Write(string message)
        {
            System.Console.Write(message);
        }

        public void WriteLine(string message)
        {
            System.Console.WriteLine(message);
        }
    }
}

public interface IAnsiSystemConsole
{
    void Write(string message);

    void WriteLine(string message);
}

public interface IConsole
{
    void Write(string message, ConsoleColor? background, ConsoleColor? foreground);
    void WriteLine(string message, ConsoleColor? background, ConsoleColor? foreground);
    void Flush();
}

/// <summary>
/// For non-Windows platform consoles which understand the ANSI escape code sequences to represent color
/// </summary>
public class AnsiLogConsole : IConsole
{
    private readonly StringBuilder _outputBuilder;
    private readonly IAnsiSystemConsole _systemConsole;

    public AnsiLogConsole(IAnsiSystemConsole systemConsole)
    {
        _outputBuilder = new StringBuilder();
        _systemConsole = systemConsole;
    }

    public void Write(string message, ConsoleColor? background, ConsoleColor? foreground)
    {
        // Order: backgroundcolor, foregroundcolor, Message, reset foregroundcolor, reset backgroundcolor
        if (background.HasValue)
        {
            _outputBuilder.Append(GetBackgroundColorEscapeCode(background.Value));
        }

        if (foreground.HasValue)
        {
            _outputBuilder.Append(GetForegroundColorEscapeCode(foreground.Value));
        }

        _outputBuilder.Append(message);

        if (foreground.HasValue)
        {
            _outputBuilder.Append("\x1B[39m\x1B[22m"); // reset to default foreground color
        }

        if (background.HasValue)
        {
            _outputBuilder.Append("\x1B[49m"); // reset to the background color
        }
    }

    public void WriteLine(string message, ConsoleColor? background, ConsoleColor? foreground)
    {
        Write(message, background, foreground);
        _outputBuilder.AppendLine();
    }

    public void Flush()
    {
        _systemConsole.Write(_outputBuilder.ToString());
        _outputBuilder.Clear();
    }

    private static string GetForegroundColorEscapeCode(ConsoleColor color)
    {
        switch (color)
        {
            case ConsoleColor.Black:
                return "\x1B[30m";
            case ConsoleColor.DarkRed:
                return "\x1B[31m";
            case ConsoleColor.DarkGreen:
                return "\x1B[32m";
            case ConsoleColor.DarkYellow:
                return "\x1B[33m";
            case ConsoleColor.DarkBlue:
                return "\x1B[34m";
            case ConsoleColor.DarkMagenta:
                return "\x1B[35m";
            case ConsoleColor.DarkCyan:
                return "\x1B[36m";
            case ConsoleColor.Gray:
                return "\x1B[37m";
            case ConsoleColor.Red:
                return "\x1B[1m\x1B[31m";
            case ConsoleColor.Green:
                return "\x1B[1m\x1B[32m";
            case ConsoleColor.Yellow:
                return "\x1B[1m\x1B[33m";
            case ConsoleColor.Blue:
                return "\x1B[1m\x1B[34m";
            case ConsoleColor.Magenta:
                return "\x1B[1m\x1B[35m";
            case ConsoleColor.Cyan:
                return "\x1B[1m\x1B[36m";
            case ConsoleColor.White:
                return "\x1B[1m\x1B[37m";
            default:
                return "\x1B[39m\x1B[22m"; // default foreground color
        }
    }

    private static string GetBackgroundColorEscapeCode(ConsoleColor color)
    {
        switch (color)
        {
            case ConsoleColor.Black:
                return "\x1B[40m";
            case ConsoleColor.Red:
                return "\x1B[41m";
            case ConsoleColor.Green:
                return "\x1B[42m";
            case ConsoleColor.Yellow:
                return "\x1B[43m";
            case ConsoleColor.Blue:
                return "\x1B[44m";
            case ConsoleColor.Magenta:
                return "\x1B[45m";
            case ConsoleColor.Cyan:
                return "\x1B[46m";
            case ConsoleColor.White:
                return "\x1B[47m";
            default:
                return "\x1B[49m"; // Use default background color
        }
    }
}

public class WindowsLogConsole : IConsole
{
    private void SetColor(ConsoleColor? background, ConsoleColor? foreground)
    {
        if (background.HasValue)
        {
            System.Console.BackgroundColor = background.Value;
        }

        if (foreground.HasValue)
        {
            System.Console.ForegroundColor = foreground.Value;
        }
    }

    private void ResetColor()
    {
        System.Console.ResetColor();
    }

    public void Write(string message, ConsoleColor? background, ConsoleColor? foreground)
    {
        SetColor(background, foreground);
        System.Console.Out.Write(message);
        ResetColor();
    }

    public void WriteLine(string message, ConsoleColor? background, ConsoleColor? foreground)
    {
        SetColor(background, foreground);
        System.Console.Out.WriteLine(message);
        ResetColor();
    }

    public void Flush()
    {
        // No action required as for every write, data is sent directly to the console
        // output stream
    }
}

public class ConsoleLoggerProcessor : IDisposable
{
    private const int _maxQueuedMessages = 1024;

    private readonly BlockingCollection<LogMessageEntry> _messageQueue = new BlockingCollection<LogMessageEntry>(_maxQueuedMessages);
    private readonly Thread _outputThread;

    public IConsole Console;

    public ConsoleLoggerProcessor()
    {
        // Start Console message queue processor
        _outputThread = new Thread(ProcessLogQueue)
        {
            IsBackground = true,
            Name = "Console logger queue processing thread"
        };
        _outputThread.Start();
    }

    public virtual void EnqueueMessage(LogMessageEntry message)
    {
        if (!_messageQueue.IsAddingCompleted)
        {
            try
            {
                _messageQueue.Add(message);
                return;
            }
            catch (InvalidOperationException) { }
        }

        // Adding is completed so just log the message
        WriteMessage(message);
    }

    // for testing
    internal virtual void WriteMessage(LogMessageEntry message)
    {
        if (message.LevelString != null)
        {
            Console.Write(message.LevelString, message.LevelBackground, message.LevelForeground);
        }

        Console.Write(message.Message, message.MessageColor, message.MessageColor);
        Console.Flush();
    }

    private void ProcessLogQueue()
    {
        try
        {
            foreach (var message in _messageQueue.GetConsumingEnumerable())
            {
                WriteMessage(message);
            }
        }
        catch
        {
            try
            {
                _messageQueue.CompleteAdding();
            }
            catch { }
        }
    }

    public void Dispose()
    {
        _messageQueue.CompleteAdding();

        try
        {
            _outputThread.Join(1500); // with timeout in-case Console is locked by user input
        }
        catch (ThreadStateException) { }
    }
}

/// <summary>
/// Default implemenation of <see cref="IExternalScopeProvider"/>
/// </summary>
public class LoggerExternalScopeProvider : IExternalScopeProvider
{
    private readonly AsyncLocal<Scope> _currentScope = new AsyncLocal<Scope>();

    /// <inheritdoc />
    public void ForEachScope<TState>(Action<object, TState> callback, TState state)
    {
        void Report(Scope current)
        {
            if (current == null)
            {
                return;
            }
            Report(current.Parent);
            callback(current.State, state);
        }
        Report(_currentScope.Value);
    }

    /// <inheritdoc />
    public IDisposable Push(object state)
    {
        var parent = _currentScope.Value;
        var newScope = new Scope(this, state, parent);
        _currentScope.Value = newScope;

        return newScope;
    }

    private class Scope : IDisposable
    {
        private readonly LoggerExternalScopeProvider _provider;
        private bool _isDisposed;

        internal Scope(LoggerExternalScopeProvider provider, object state, Scope parent)
        {
            _provider = provider;
            State = state;
            Parent = parent;
        }

        public Scope Parent { get; }

        public object State { get; }

        public override string ToString()
        {
            return State?.ToString();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _provider._currentScope.Value = Parent;
                _isDisposed = true;
            }
        }
    }
}

#endif
