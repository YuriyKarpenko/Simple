# Simple.Ttl

This project about Time To Live (TTL) of variable. After a given period of time, the variable updates its value from the factory method.

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
### 8.0.0.5
	append:
		TtlBackground
		support .Net8.0
	remove:
		TtlLazy

### 0.1.4.4
	fixed:
		asynchronous method TtlValue.GetOrCreateAsync
	append:
		support .Net6.0
		expected unit tests

### 0.1.3.3
	fixed:
		asynchronous method TtlValue.GetOrCreateAsync

### 0.1.2.2
	update:
		small fixes + refactoring classes TtlDictionary and TtlValues
