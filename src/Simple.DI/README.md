#	Simple.DI

Pierian spring:
https://github.com/reactiveui/splat
and using like a Splat.Locator, but has little different names

##	Using

```cs
	using Simple.DI;
	static void InitDi()
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
	using Simple.DI;
	private readonly IRepoSomething _repo;
	public AnyService()
	{
		_repo = Locator.Current.GetService<IRepoSomething>();
	}
```

##	Release notes

###	8.0.1.4

	use AI analize

	append:
		scope disposal: Dispose releases IDisposable services registered inside the scope (reverse creation order; every transient instance too); root registrations are never disposed
		AddScopedSingleton overloads: one cached instance per registration via shared memoize helper (thread-safe); registration lives in current scope or root
		Resolver.GetService(Type): a closed generic key falls back to its generic type definition (ILogget&lt;User&gt; → ILogget&lt;&gt;), exact registration wins; logic moved here from the removed Locator.GetServiceGeneric
	fix:
		scope stack is per-container (was: shared static per TKey — one container's CreateScope leaked into another)
		TryRegister: atomic add (was: check-then-register race could overwrite a concurrent registration)
		AddTransient(Type) registers in the root (was: via RegisterScoped — a transient registered inside a scope vanished after Dispose; inconsistent with AddTransient(Func))
		Scope.Dispose: out-of-order dispose of an ancestor unwinds its descendants; late dispose of an unwound scope no longer resurrects a stale scope onto the stack
		GetServices(Predicate): TryGetValue guard instead of direct dictionary lookup
		Locator.Setup(Action): ArgumentNullException for a null action (was: NullReferenceException)
		AddScoped: all overloads are transient again — factory runs per resolve (was: Type-overloads cached via instance ??= = singleton at startup registration)
		AddSingleton factory overloads: thread-safe first create via shared memoize helper (double-checked lock; was: instance ??= race could create two instances)
		Scope.Track after scope Dispose: no orphan disposables list (was: theoretical zombie tracking)
	update:
		XML-doc for the public API (GenerateDocumentationFile on)
		README examples fixed (namespace Simple.DI, valid InitDi signature)
		transient/scoped Type overloads: parameterless ctor compiled once per registration via expression tree (falls back to Activator for abstract/interface/no-ctor types)
		Resolver<TKey> ctor: parentResolver default null (new Resolver<TKey>() compiles; parity with Resolver)
		unit-tests: 11 => 51
	note:breaking:
		obsolete DiExtensions.AddConst removed — use AddSingleton<I>
		GetServices<T> extension throws InvalidOperationException when the provider is not a Resolver (was: silent empty result)
		Locator.GetServiceGeneric removed (was Obsolete "Will be removed") — logic lives in Resolver.GetService(Type) fallback (exact key → generic type definition)

###	8.0.0.3

	append:
		support scope
		support net8.0
	update:
		refactoring

###	0.1.2.2

	append:
		support net6.0

###	0.1.1.1

	update:
		Small improvements to the hierarchy of classes
