# Simple.Ttl

Very simple and fast JWT.

##	Using
```cs
private IRepoConfig _repo = ...
private readonly TtlValue _configApp = new TtlValue(TimeSpan.FromMinutes(5));
...
public IConfigApp GetAppConfig()
	=> _configApp.GetOrCreate(() => new ConfigApp(_repo.Get...(...)));
```
OR
```cs
private Task<IConfigApp> CreateConfigAsync(){...}
...
public Task<IConfigApp> GetAppConfigAsync()
	=> _configApp.GetOrCreateAsync(CreateConfigAsync);
```
##  Release notes
### 0.0.2.2
	fix:
		generate "NotBefore" and "IssuedAt" always
	update:
		extending error messages

### 0.0.1.1
	update:
		start
