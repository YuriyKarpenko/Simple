using System;

namespace Simple.DI
{
    public interface IProviderSetup<TKey>
    {
        /// <summary> Try to registering factory of object </summary>
        bool TryRegister(TKey key, Func<object?> factory);

        /// <summary> Registering factory of object </summary>
        IProviderSetup<TKey> Register(TKey key, Func<object?> factory);

        /// <summary> Create a new <see cref="IProviderSetup"/> based on current (current will not be modified) </summary>
        IProviderSetup<TKey> CreateScope();
    }

    public interface IProviderSetup : IProviderSetup<Type>
    {
        /// <summary> Registering factory of object </summary>
        IProviderSetup Register(Type key, Func<IServiceProvider, object?> factory);

        /// <summary> Creating a type-resolver </summary>
        IServiceProvider BuildServiceProvider();

        /// <inheritdoc />
        new IProviderSetup CreateScope();
    }
}