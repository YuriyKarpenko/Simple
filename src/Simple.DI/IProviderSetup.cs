using System;
using System.Collections.Generic;

namespace Simple.DI;

/// <summary> Registration surface over a key-based resolver: factories, scopes and service enumeration </summary>
public interface IProviderSetup<TKey>
{
    /// <summary> Try to registering factory of object </summary>
    bool TryRegister(TKey key, Func<object?> factory);

    /// <summary> Registering factory of object </summary>
    void Register(TKey key, Func<object?> factory);

    /// <summary> Registering factory of object </summary>
    void RegisterScoped(TKey key, Func<object?> factory);

    /// <summary> Creating IDisposable object to capture scoped resolver </summary>
    IDisposable CreateScope();

    /// <summary> Enumerates services registered for keys matching the predicate </summary>
    IEnumerable<object?> GetServices(Predicate<TKey> predicate);
}

/// <summary> Type-keyed registration surface: <see cref="IServiceProvider" /> with typed factories and enumeration </summary>
public interface IProviderSetup : IProviderSetup<Type>, IServiceProvider
{
    /// <summary> Registering factory of object with IServiceProvider </summary>
    void Register(Type key, Func<IServiceProvider, object?> factory);

    /// <summary> Registering factory of object with IServiceProvider </summary>
    void RegisterScoped(Type key, Func<IServiceProvider, object?> factory);

    /// <summary> Enumerates services whose keys are assignable to the base type </summary>
    IEnumerable<object?> GetServices(Type baseType);

    /// <summary> Creating a type-resolver </summary>
    IServiceProvider BuildServiceProvider();
}