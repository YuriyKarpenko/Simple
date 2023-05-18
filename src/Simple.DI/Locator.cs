using System;

namespace Simple.DI
{
    public class Locator : Resolver<Type>, IProviderSetup, IServiceProvider
    {
        private static readonly Locator _instance = new();

        /// <summary> Gets the <see cref="IServiceProvider" /> that provides access to the application's service container </summary>
        public static IServiceProvider Current => _instance;

        /// <summary> Allows registering the application's service factory <see cref="IProviderSetup" /> </summary>
        public static IProviderSetup Setup() => _instance;


        protected Locator() : base(null)
        {
        }

        #region IProviderSetup

        public IProviderSetup Register(Type key, Func<IServiceProvider, object?> factory)
            => (IProviderSetup)base.Register(key, () => factory(this));

        public IServiceProvider BuildServiceProvider()
            => this;

        #endregion

        #region IServiceProvider

        //  Replacing the key from 'ILogget<T>' on 'ILogget<>'
        public override object? GetService(Type key)
            => base.GetService(key.IsGenericType ? key.GetGenericTypeDefinition() : key);

        #endregion
    }
}