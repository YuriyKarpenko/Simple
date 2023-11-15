using System;

namespace Simple.DI
{
    public class Locator
    {
        private static readonly Resolver _instance = new();

        /// <summary> Gets the <see cref="IServiceProvider" /> that provides access to the application's service container </summary>
        public static IServiceProvider Current => _instance;

        /// <summary> Allows registering the application's service factory <see cref="IProviderSetup" /> </summary>
        public static IProviderSetup Setup() => _instance;


        #region IServiceProvider

        //  Replacing the key from 'ILogget<T>' on 'ILogget<>'
        public object? GetServiceGeneric(Type key)
            => _instance.GetService(key.IsGenericType ? key.GetGenericTypeDefinition() : key);

        #endregion
    }
}