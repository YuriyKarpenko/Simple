using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Simple.DI;

public class Resolver<TKey>(Resolver<TKey>? parentResolver) : IProviderSetup<TKey> where TKey : notnull
{
    protected static readonly ScopeProvider<Resolver<TKey>> _scopeProvider = new();

    private readonly Resolver<TKey>? _parentResolver = parentResolver;
    private readonly ConcurrentDictionary<TKey, Func<object?>> _registry = new();


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
        return _parentResolver?.GetService(key);
    }

    #endregion

    #region IProviderSetup

    /// <inheritdoc />
    public virtual IDisposable CreateScope()
        => _scopeProvider.Push(new Resolver<TKey>(ResolverScoped));

    /// <inheritdoc />
    public virtual bool TryRegister(TKey key, Func<object?> factory)
    {
        if (_registry.TryGetValue(key, out _))
        {
            return false;
        }
        Register(key, factory);
        return true;
    }

    /// <inheritdoc />
    public virtual void Register(TKey key, Func<object?> factory) 
        => _registry[key] = factory;

    public virtual void RegisterScoped(TKey key, Func<object?> factory)
        => ResolverScoped.Register(key, factory);

    public IEnumerable<object?> GetServices(Predicate<TKey> predicate)
    {
        foreach (var res in AllResolvers())
        {
            var reg = res._registry;
            foreach (var t in reg.Keys)
            {
                if (predicate(t))
                {
                    yield return reg[t]();
                }
            }
        }
    }

    #endregion

    protected IEnumerable<Resolver<TKey>> AllResolvers()
    {
        var res = ResolverScoped;
        while (res != null)
        {
            yield return res;
            res = res._parentResolver;
        }
    }
}

public class Resolver(Resolver? parentResolver = null) : Resolver<Type>(parentResolver), IProviderSetup, IServiceProvider
{
    public Resolver ScopedSp => (Resolver)base.ResolverScoped;

    #region IProviderSetup

    public override IDisposable CreateScope()
        => _scopeProvider.Push(new Resolver(ScopedSp));

    public void Register(Type key, Func<IServiceProvider, object?> factory)
        => base.Register(key, () => factory(this));

    public void RegisterScoped(Type key, Func<IServiceProvider, object?> factory)
        => ScopedSp.Register(key, factory);

    public IServiceProvider BuildServiceProvider()
        => this;

    public IEnumerable<object?> GetServices(Type baseType) 
        => GetServices(baseType.IsAssignableFrom);

    #endregion
}