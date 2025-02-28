using System;

namespace Simple.DI;

public static class DiExtensions
{
    public static T? GetService<T>(this IServiceProvider resolver)
        => (T?)resolver?.GetService(typeof(T));

    [Obsolete("Will be removed")]
    public static IProviderSetup AddConst<I>(this IProviderSetup setup, I instance)
        => AddSingleton<I>(setup, instance);

    //  Singleton
    public static IProviderSetup AddSingleton<I>(this IProviderSetup setup, I instance)
        => UsingSetup(setup, s => s.Register(typeof(I), () => instance));

    public static IProviderSetup AddSingleton<I>(this IProviderSetup setup, Func<IServiceProvider, I> factory)
    {
        I? instance = default;
        return UsingSetup(setup, s => s.Register(typeof(I), sp => (instance ??= factory(sp))));
    }

    public static IProviderSetup AddSingleton<I>(this IProviderSetup setup, Func<I> factory)
        => AddSingleton<I>(setup, _ => factory());

    public static IProviderSetup AddSingleton<I, T>(this IProviderSetup setup) where T : class, I, new()
        => AddSingleton<I>(setup, _ => new T());

    public static IProviderSetup AddSingleton<T>(this IProviderSetup setup) where T : class, new()
        => AddSingleton<T, T>(setup);

    //  Scoped
    public static IProviderSetup AddScoped<I>(this IProviderSetup setup, Func<IServiceProvider, I> factory)
        => UsingSetup(setup, s => s.RegisterScoped(typeof(I), sp => factory(sp)));

    public static IProviderSetup AddScoped<I>(this IProviderSetup setup, Func<I> factory)
        => AddScoped<I>(setup, _ => factory());

    public static IProviderSetup AddScoped<I, T>(this IProviderSetup setup) where T : class, I, new()
        => UsingSetup(setup, s => s.RegisterScoped(typeof(I), () => new T()));

    public static IProviderSetup AddScoped<T>(this IProviderSetup setup) where T : class, new()
        => UsingSetup(setup, s => s.RegisterScoped(typeof(T), () => new T()));

    //  Transient
    public static IProviderSetup AddTransient<I>(this IProviderSetup setup, Func<I> factory)
        => UsingSetup(setup, s => s.Register(typeof(I), _ => factory()));

    public static IProviderSetup AddTransient<I>(this IProviderSetup setup, Func<IServiceProvider, I> factory)
        => UsingSetup(setup, s => s.Register(typeof(I), sp => factory(sp)));

    public static IProviderSetup AddTransient<I, T>(this IProviderSetup setup) where T : class, I, new()
        => AddTransient<I>(setup, () => new T());

    //  check for null
    public static IProviderSetup UsingSetup(IProviderSetup setup, Action<IProviderSetup> register)
    {
        if (setup != null)
        {
            if (register != null)
            {
                register(setup);
                return setup;
            }
            throw new ArgumentNullException(nameof(register));
        }
        throw new ArgumentNullException(nameof(setup));
    }
}