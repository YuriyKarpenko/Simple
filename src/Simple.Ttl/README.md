#	Simple.Ttl

This project about Time To Live (TTL) of variable. After a given period of time, the variable updates its value from the factory method.

##	Using

```cs
private IRepoConfig _repo = ...
private readonly TtlValue<IConfigApp> _configApp = new TtlValue<IConfigApp>(TimeSpan.FromMinutes(5));
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

##	Release notes

###	8.0.1.6

	use AI analize

	append:
		stale-while-revalidate option: TtlValue, TtlDictionary, TtlValues constructors
		TtlBackgroundAsync<TArg, TRes>: async factory with CancellationToken, background refresh
		LastRefreshError on TtlBackground and TtlBackgroundAsync (last refresh failure, null after success)
	fixed:
		GetOrCreateAsync cold start - single factory start under lock
		factory exception propagates to the awaiting caller
		ttl bump only on success (ttl counts from value readiness)
		GetOrCreate: blocking factory runs outside _lock (slow factory no longer stalls Set/GetOrCreateAsync)
		SetAsync resets single-flight marker only for its own task
		TtlBackground: reentrancy guard + atomic Value publication (no parallel factory runs)
		TtlBackground: idempotent Dispose, first tick after ttl, timer callback never throws after Dispose
		TtlBackgroundAsync: a refresh result arriving after Dispose is not published
		TtlBase: expiry uses a monotonic clock (Stopwatch) - wall clock jumps do not affect ttl
	update:
		explicit 1-arg constructors (binary compatibility)
		unit tests 20 => 56
		XML comments on the public API
		TtlValue.SetAsync is private now (API)
		TtlBackground<T> => TtlBackground<TArg, T>: sync factory, Timer-driven (breaking)
		ThreadTimerAsync: CancellationToken in readonly field (no boxing, safe after Dispose)
	remove:
		TtlBackgroundTimer<T> (PeriodicTimer-based)

###	8.0.0.5

	append:
		TtlBackground
		support .Net8.0
	remove:
		TtlLazy

###	0.1.4.4

	fixed:
		asynchronous method TtlValue.GetOrCreateAsync
	append:
		support .Net6.0
		expected unit tests

###	0.1.3.3

	fixed:
		asynchronous method TtlValue.GetOrCreateAsync

###	0.1.2.2

	update:
		small fixes + refactoring classes TtlDictionary and TtlValues
