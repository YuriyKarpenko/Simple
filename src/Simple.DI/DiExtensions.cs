using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;

namespace Simple.DI;

/// <summary> Fluent registration extensions for <see cref="IProviderSetup" />: singleton, scoped and transient lifetimes </summary>
public static class DiExtensions
{
    /// <summary> Resolves the service of type <typeparamref name="T" /> (null when not registered) </summary>
    public static T? GetService<T>(this IServiceProvider resolver)
        => (T?)resolver?.GetService(typeof(T));

    /// <summary> Enumerates services assignable to <typeparamref name="T" />; throws <see cref="InvalidOperationException" /> when the provider is not a <see cref="Resolver" /> </summary>
    public static IEnumerable<object?> GetServices<T>(this IServiceProvider resolver)
        => resolver is Resolver r
            ? r.GetServices(typeof(T))
            : throw new InvalidOperationException($"GetServices<{typeof(T).Name}> requires a Resolver provider, but got {resolver?.GetType().Name ?? "null"}");

    //  Singleton
    /// <summary> Registers the given instance as singleton </summary>
    public static IProviderSetup AddSingleton<I>(this IProviderSetup setup, I instance)
        => UsingSetup(setup, s => s.Register(typeof(I), () => instance));

    /// <summary> Registers a singleton of the type: one instance for the container lifetime (thread-safe first create) </summary>
    public static IProviderSetup AddSingleton(this IProviderSetup setup, Type t)
        => UsingSetup(setup, s => s.Register(t, Memoize(_ => Activator.CreateInstance(t))));

    /// <summary> Registers <typeparamref name="I" /> as singleton of the type: one instance for the container lifetime (thread-safe first create) </summary>
    public static IProviderSetup AddSingleton<I>(this IProviderSetup setup, Type t)
        => UsingSetup(setup, s => s.Register(typeof(I), Memoize(_ => (I?)Activator.CreateInstance(t))));

    /// <summary> Registers a singleton by factory: one instance for the container lifetime (thread-safe first create) </summary>
    public static IProviderSetup AddSingleton<I>(this IProviderSetup setup, Func<IServiceProvider, I> factory)
        => UsingSetup(setup, s => s.Register(typeof(I), Memoize(sp => factory(sp))));

    /// <summary> Registers a singleton by factory: one instance for the container lifetime (thread-safe first create) </summary>
    public static IProviderSetup AddSingleton<I>(this IProviderSetup setup, Func<I> factory)
        => AddSingleton<I>(setup, _ => factory());

    /// <summary> Registers a singleton of <typeparamref name="T" /> created once by its parameterless constructor </summary>
    public static IProviderSetup AddSingleton<I, T>(this IProviderSetup setup) where T : class, I, new()
        => AddSingleton<I>(setup, _ => new T());

    /// <summary> Registers a singleton of <typeparamref name="T" /> created once by its parameterless constructor </summary>
    public static IProviderSetup AddSingleton<T>(this IProviderSetup setup) where T : class, new()
        => AddSingleton<T, T>(setup);

    //  Scoped
    /// <summary> Registers in the current scope: the factory runs per resolve, the registration dies with the scope </summary>
    public static IProviderSetup AddScoped(this IProviderSetup setup, Type t)
        => UsingSetup(setup, s => s.RegisterScoped(t, CtorFactory(t)));

    /// <summary> Registers <typeparamref name="I" /> in the current scope: the factory runs per resolve, the registration dies with the scope </summary>
    public static IProviderSetup AddScoped<I>(this IProviderSetup setup, Type t)
        => UsingSetup(setup, s =>
        {
            var create = CtorFactory(t);
            s.RegisterScoped(typeof(I), sp => (I)create(sp)!);
        });

    /// <summary> Registers in the current scope: the factory runs per resolve, the registration dies with the scope </summary>
    public static IProviderSetup AddScoped<I>(this IProviderSetup setup, Func<IServiceProvider, I> factory)
        => UsingSetup(setup, s => s.RegisterScoped(typeof(I), sp => factory(sp)));

    /// <summary> Registers in the current scope: the factory runs per resolve, the registration dies with the scope </summary>
    public static IProviderSetup AddScoped<I>(this IProviderSetup setup, Func<I> factory)
        => AddScoped<I>(setup, _ => factory());

    /// <summary> Registers <typeparamref name="T" /> in the current scope: the factory runs per resolve, the registration dies with the scope </summary>
    public static IProviderSetup AddScoped<I, T>(this IProviderSetup setup) where T : class, I, new()
        => UsingSetup(setup, s => s.RegisterScoped(typeof(I), () => new T()));

    /// <summary> Registers <typeparamref name="T" /> in the current scope: the factory runs per resolve, the registration dies with the scope </summary>
    public static IProviderSetup AddScoped<T>(this IProviderSetup setup) where T : class, new()
        => AddScoped<T, T>(setup);

    //  Scoped singleton
    /// <summary> Registers in the current scope with one cached instance per registration (thread-safe first create) </summary>
    public static IProviderSetup AddScopedSingleton(this IProviderSetup setup, Type t)
        => UsingSetup(setup, s => s.RegisterScoped(t, Memoize(_ => Activator.CreateInstance(t))));

    /// <summary> Registers <typeparamref name="I" /> in the current scope with one cached instance per registration (thread-safe first create) </summary>
    public static IProviderSetup AddScopedSingleton<I>(this IProviderSetup setup, Type t)
        => UsingSetup(setup, s => s.RegisterScoped(typeof(I), Memoize(_ => (I?)Activator.CreateInstance(t))));

    /// <summary> Registers in the current scope with one cached instance per registration (thread-safe first create) </summary>
    public static IProviderSetup AddScopedSingleton<I>(this IProviderSetup setup, Func<IServiceProvider, I> factory)
        => UsingSetup(setup, s => s.RegisterScoped(typeof(I), Memoize(sp => factory(sp))));

    /// <summary> Registers in the current scope with one cached instance per registration (thread-safe first create) </summary>
    public static IProviderSetup AddScopedSingleton<I>(this IProviderSetup setup, Func<I> factory)
        => AddScopedSingleton<I>(setup, _ => factory());

    /// <summary> Registers <typeparamref name="T" /> in the current scope with one cached instance per registration (thread-safe first create) </summary>
    public static IProviderSetup AddScopedSingleton<I, T>(this IProviderSetup setup) where T : class, I, new()
        => AddScopedSingleton<I>(setup, _ => new T());

    /// <summary> Registers <typeparamref name="T" /> in the current scope with one cached instance per registration (thread-safe first create) </summary>
    public static IProviderSetup AddScopedSingleton<T>(this IProviderSetup setup) where T : class, new()
        => AddScopedSingleton<T, T>(setup);

    //  Transient
    /// <summary> Registers a transient: a new instance per resolve, the registration lives in the root </summary>
    public static IProviderSetup AddTransient(this IProviderSetup setup, Type t)
        => UsingSetup(setup, s => s.Register(t, CtorFactory(t)));

    /// <summary> Registers <typeparamref name="I" /> as transient: a new instance per resolve, the registration lives in the root </summary>
    public static IProviderSetup AddTransient<I>(this IProviderSetup setup, Type t)
        => UsingSetup(setup, s =>
        {
            var create = CtorFactory(t);
            s.Register(typeof(I), sp => (I)create(sp)!);
        });

    /// <summary> Registers a transient by factory: a new instance per resolve, the registration lives in the root </summary>
    public static IProviderSetup AddTransient<I>(this IProviderSetup setup, Func<I> factory)
        => UsingSetup(setup, s => s.Register(typeof(I), _ => factory()));

    /// <summary> Registers a transient by factory: a new instance per resolve, the registration lives in the root </summary>
    public static IProviderSetup AddTransient<I>(this IProviderSetup setup, Func<IServiceProvider, I> factory)
        => UsingSetup(setup, s => s.Register(typeof(I), sp => factory(sp)));

    /// <summary> Registers <typeparamref name="T" /> as transient: a new instance per resolve, the registration lives in the root </summary>
    public static IProviderSetup AddTransient<I, T>(this IProviderSetup setup) where T : class, I, new()
        => AddTransient<I>(setup, () => new T());

    //  check for null
    /// <summary> Validates <paramref name="setup" /> and <paramref name="register" />, runs the registration and returns the setup </summary>
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

    private static Func<IServiceProvider, object?> CtorFactory(Type t)
    {
        Func<IServiceProvider, object?> fallback = _ => Activator.CreateInstance(t);
        if (t.IsValueType)
        {
            return fallback;
        }
        try
        {
            var sp = Expression.Parameter(typeof(IServiceProvider), "sp");
            return Expression.Lambda<Func<IServiceProvider, object?>>(Expression.New(t), sp).Compile();
        }
        catch (Exception)
        {
            return fallback;
        }
    }

    private static Func<IServiceProvider, object?> Memoize(Func<IServiceProvider, object?> factory)
    {
        object? instance    = null;
        var     gate        = new object();
        var     initialized = 0;
        return sp =>
        {
            if (Volatile.Read(ref initialized) == 0)
            {
                lock (gate)
                {
                    if (initialized == 0)
                    {
                        instance = factory(sp);
                        Volatile.Write(ref initialized, 1);
                    }
                }
            }
            return instance;
        };
    }
}