using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Simple.DI;

/// <summary> Key-based service resolver: registry with parent-chain fallback and AsyncLocal scopes; scope owns disposable tracking </summary>
public class Resolver<TKey>(Resolver<TKey>? parentResolver = null) : IProviderSetup<TKey> where TKey : notnull
{
    /// <summary> Scope stack of this container (per-instance, flows with the async execution context) </summary>
    protected readonly ScopeProvider<Resolver<TKey>> _scopeProvider = new();

    private readonly Resolver<TKey>?                           _parentResolver = parentResolver;
    private readonly ConcurrentDictionary<TKey, Func<object?>> _registry       = new();


    /// <summary> Resolver of the current AsyncLocal scope, or this when no scope is active </summary>
    public Resolver<TKey> ResolverScoped => _scopeProvider.Current ?? this;


    #region IServiceProvider

    /// <inheritdoc />
    public virtual object? GetService(TKey key)
    {
        var r = ResolverScoped;
        while (r != null)
        {
            if (r._registry.TryGetValue(key, out var factory))
            {
                return factory();
            }
            r = r._parentResolver;
        }
        return null;
    }

    #endregion

    #region IProviderSetup

    /// <inheritdoc />
    public virtual IDisposable CreateScope()
        => _scopeProvider.Push(new Resolver<TKey>(ResolverScoped));

    /// <inheritdoc />
    public virtual bool TryRegister(TKey key, Func<object?> factory)
        => _registry.TryAdd(key, factory);

    /// <inheritdoc />
    public virtual void Register(TKey key, Func<object?> factory)
        => _registry[key] = factory;

    /// <summary> Registers the factory in the current scope; created IDisposable instances are tracked by the scope and disposed with it </summary>
    public virtual void RegisterScoped(TKey key, Func<object?> factory)
    {
        var scope = _scopeProvider.CurrentScope;
        ResolverScoped.Register(key, scope != null ? Track(factory, scope.Track) : factory);
    }

    /// <summary> Enumerates services matching the predicate over the current scope chain </summary>
    public IEnumerable<object?> GetServices(Predicate<TKey> predicate)
    {
        foreach (var res in AllResolvers())
        {
            var reg = res._registry;
            foreach (var t in reg.Keys)
            {
                if (predicate(t) && reg.TryGetValue(t, out var factory))
                {
                    yield return factory();
                }
            }
        }
    }

    #endregion

    /// <summary> Yields this resolver followed by its parent chain </summary>
    protected IEnumerable<Resolver<TKey>> AllResolvers()
    {
        var res = ResolverScoped;
        while (res != null)
        {
            yield return res;

            res = res._parentResolver;
        }
    }

    private static Func<object?> Track(Func<object?> factory, Action<object?> track)
        => () =>
        {
            var instance = factory();
            track(instance);
            return instance;
        };
}

/// <summary> Type-keyed resolver: <see cref="IServiceProvider" /> with scopes, parent-chain fallback and scope-owned disposable tracking </summary>
public class Resolver(Resolver? parentResolver = null) : Resolver<Type>(parentResolver), IProviderSetup, IServiceProvider
{
    /// <summary> Resolver of the current scope, cast to the non-generic type </summary>
    public Resolver ScopedSp => (Resolver)ResolverScoped;

    #region IProviderSetup

    /// <inheritdoc />
    public override IDisposable CreateScope()
        => _scopeProvider.Push(new Resolver(ScopedSp));

    /// <summary> Resolves the service; a closed generic key falls back to its generic type definition (ILogget&lt;User&gt; → ILogget&lt;&gt;), exact registration wins </summary>
    public override object? GetService(Type key)
        => key.IsGenericType
            ? base.GetService(key) ?? base.GetService(key.GetGenericTypeDefinition())
            : base.GetService(key);

    /// <inheritdoc />
    public void Register(Type key, Func<IServiceProvider, object?> factory)
        => base.Register(key, () => factory(this));

    /// <inheritdoc />
    public void RegisterScoped(Type key, Func<IServiceProvider, object?> factory)
        => base.RegisterScoped(key, () => factory(this));

    /// <inheritdoc />
    public IServiceProvider BuildServiceProvider()
        => this;

    /// <inheritdoc />
    public IEnumerable<object?> GetServices(Type baseType)
        => GetServices(baseType.IsAssignableFrom);

    #endregion

}