using System;

namespace Simple.DI;

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
}

public interface IProviderSetup : IProviderSetup<Type>, IServiceProvider
{
    /// <summary> Registering factory of object with IServiceProvider </summary>
    void Register(Type key, Func<IServiceProvider, object?> factory);

    /// <summary> Registering factory of object with IServiceProvider </summary>
    void RegisterScoped(Type key, Func<IServiceProvider, object?> factory);

    /// <summary> Creating a type-resolver </summary>
    IServiceProvider BuildServiceProvider();
}