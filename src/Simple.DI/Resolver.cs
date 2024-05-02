using System;
using System.Collections.Concurrent;

namespace Simple.DI
{
    public class Resolver<TKey> : IProviderSetup<TKey> where TKey : notnull
    {
        private readonly Resolver<TKey>? _parentResolver;
        private readonly ConcurrentDictionary<TKey, Func<object?>> _registry = new();

        protected Resolver(Resolver<TKey>? parentResolver)
        {
            _parentResolver = parentResolver;
        }

        #region IServiceProvider

        /// <inheritdoc />
        public virtual object? GetService(TKey key)
        {
            return _registry.TryGetValue(key, out var factory)
                ? factory()
                : _parentResolver?.GetService(key);
        }

        public IProviderSetup<TKey> CreateScope()
            => new Resolver<TKey>(this);

        #endregion


        #region IProviderSetup

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
        public virtual IProviderSetup<TKey> Register(TKey key, Func<object?> factory)
        {
            _registry[key] = factory;
            return this;
        }

        #endregion
    }

    public class Resolver : Resolver<Type>, IProviderSetup, IServiceProvider
    {
        public Resolver(Resolver<Type>? parentResolver = null) : base(parentResolver)
        {
        }

        #region IProviderSetup

        public new IProviderSetup CreateScope()
            => new Resolver(this);

        public IProviderSetup Register(Type key, Func<IServiceProvider, object?> factory)
            => (IProviderSetup)base.Register(key, () => factory(this));

        public IServiceProvider BuildServiceProvider()
            => this;

        #endregion
    }
}