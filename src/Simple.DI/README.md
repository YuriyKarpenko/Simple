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

##  Release notes
0.1.2.2
	append:
		support net6.0
0.1.1.1		
	update:
		Small improvements to the hierarchy of classes
