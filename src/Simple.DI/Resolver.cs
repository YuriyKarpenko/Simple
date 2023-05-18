using System;
using System.Collections.Concurrent;

namespace Simple.DI
{
    public class Resolver<TKey> : IProviderSetup<TKey>
    {
        private readonly Resolver<TKey>? _resolverParent;
        private readonly ConcurrentDictionary<TKey, Func<object?>> _registry = new();

        protected Resolver(Resolver<TKey>? registry)
        {
            _resolverParent = registry;
        }

        #region IServiceProvider

        /// <inheritdoc />
        public virtual object? GetService(TKey key)
        {
            if (_registry.TryGetValue(key, out var factory))
            {
                return factory();
            }
            return _resolverParent?.GetService(key);
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
}