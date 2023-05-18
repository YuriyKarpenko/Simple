using System;

namespace Simple.DI
{
    public static class DiExtensions
    {
        public static T? GetService<T>(this IServiceProvider resolver)
            => (T?)resolver?.GetService(typeof(T));


        public static IProviderSetup AddConst<I>(this IProviderSetup setup, I instance)
        {
            return UsingSetup(setup, s => s.Register(typeof(I), sp => instance));
        }

        public static IProviderSetup AddSingleton<I>(this IProviderSetup setup, I instance)
            => AddConst(setup, instance);
        public static IProviderSetup AddSingleton<I>(this IProviderSetup setup, Func<IServiceProvider, I> factory)
        {
            I? instance = default;
            return UsingSetup(setup, s => s.Register(typeof(I), sp => instance ?? (instance = factory(sp))));
        }
        public static IProviderSetup AddSingleton<I>(this IProviderSetup setup, Func<I> factory)
        {
            I? instance = default;
            return UsingSetup(setup, s => s.Register(typeof(I), sp => instance ?? (instance = factory())));
        }
        public static IProviderSetup AddSingleton<I, T>(this IProviderSetup setup) where T : class, I, new()
            => AddSingleton<I>(setup, () => new T());
        public static IProviderSetup AddSingleton<T>(this IProviderSetup setup) where T : class, new()
            => AddSingleton<T, T>(setup);

        //  Scoped is fake
        public static IProviderSetup AddScoped<I>(this IProviderSetup setup, Func<IServiceProvider, I> factory)
            => AddSingleton<I>(setup, factory);
        public static IProviderSetup AddScoped<I>(this IProviderSetup setup, Func<I> factory)
            => AddSingleton<I>(setup, factory);
        public static IProviderSetup AddScoped<I, T>(this IProviderSetup setup) where T : class, I, new()
            => AddSingleton<I>(setup, () => new T());

        public static IProviderSetup AddTransient<I>(this IProviderSetup setup, Func<I> factory)
            => UsingSetup(setup, s => s.Register(typeof(I), sp => factory()));
        public static IProviderSetup AddTransient<I>(this IProviderSetup setup, Func<IServiceProvider, I> factory)
            => UsingSetup(setup, s => s.Register(typeof(I), sp => factory(sp)));
        public static IProviderSetup AddTransient<I, T>(this IProviderSetup setup) where T : class, I, new()
            => AddTransient<I>(setup, () => new T());


        public static IProviderSetup UsingSetup(IProviderSetup setup, Action<IProviderSetup> register)
        {
            if (setup == null)
            {
                throw new ArgumentNullException(nameof(setup));
            }
            register(setup);
            return setup;
        }
    }
}
