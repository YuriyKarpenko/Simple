# Simple

## Throw

Pierian spring:
https://reubenbond.github.io/posts/dotnet-perf-tuning
or translation
https://www.lshnk.me/2019/01/26/улучшаем-производительность-в-.net-core-по-опыту-бинарного-сериализатора-hagar/

### Using

```cs
using Simple;
...
	Throw.Exception(new ...Exception(parameters...));
...
	//	allows optional additional check
	Throw.IsArgumentNullException(file, nameof(file), i => !string.IsNullOrEmpty(i));
```

## HashCoceCombiner 
class (from MS v2.2)

## Extensions

some extensions for Stream and collectopns


# Simple.DI

Pierian spring:
https://github.com/reactiveui/splat
and using like a Splat.Locator, but has litle different names

##	Using
```cs
	using Dimple.DI;
	static InitDi()
	{
		var db = new SomethingDataBase(...);
		var setup = Locator.Setup();
		setup
			.AddSingleton(db)
			.AddSingleton<IRepoSomething>(() => new RepoSomething(db))
			//or	.AddSingleton<IRepoSomething>(sp => new RepoSomething(sp.GetService<SomethingDataBase>()))
			.AddSingleton<IOthers>(...) ...
	}
...
	using Dimple.DI;
	private readonly IRepoSomething _repo;
	public AnyService()
	{
		_repo = Locator.Current.GetService<IRepoSomething>();
	}
```


# Simple.Configuration

Easy way to use JSON and Envirement parameters, which used like a MS in "Hosting" flow

##	Using
```cs
    var builder = new ConfigurationBuilder()
        .SetBasePath(context.HostingEnvironment.ContentRootPath)
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

    var configuration = builder.Build();

	Locator.Setup().AddConst<IConfiguration>(configuration);
...
    var configuration = Locator.Current.GetService<IConfiguration>();
	var myConfig = configuration.Json.GetValue("some section").ToObject<MyConfig>();
```

# Simple.Ttl

##	Using
```cs
private IRepoConfig _repo = ...
private readonly TtlValue _configApp(TimeSpan.FromMinutes(5));

...

public IConfigApp GetAppConfig()
	=> _configApp.GetOrCreate(() => new ConfigApp(_repo.Get...(...)));
```
OR
```cs
public Task<IConfigApp> GetAppConfigAsync()
	=> _configApp.GetOrCreateAsync(() => Get...Async(...));
```

