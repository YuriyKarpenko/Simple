# Simple.Logging


##	Using
```cs
using Simple.Logging;
	...
	LogManager.Option.SetMinLevel(LogLevel.Info).AddDebug().AddConsole();
	...
	private readonly ILogger _logger;
	public ClassCtor(IServiceProvider sp)
	{
		_logger = sp.CreateLogger<ClassCtor>();
		// OR
		// _logger = sp.CreateLogger(typeof(ClassCtor));
	}
	...
```

##  Release notes
8.0.0.5
	Append:
		support net8.0
	Update:
		LoggerExtensions
		scopes
0.1.4
	Append:
		use 'Microsoft.Extensions.Logging' (remove some our intrfaces and enum)
	Update:
		LoggerExtensions
		some name-spaces
		some **Factory names
0.1.3
	refactoring to easily reflect config.json to logger options
0.1.2
	Append:
		implement scope
	Update:
		Configurations
		ObservableConsole
0.1.1
	Update:
		logSource changed (Type => string)
		remove "EventId" from ILogger
0.1.0
	start
