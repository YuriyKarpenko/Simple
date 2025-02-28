using Simple.DI;

namespace Simple.Hosting;

//  TODO: need implement in Simple.DI
public static class ExtensionsDi
{
    public static IProviderSetup TryAddSingleton<I>(this IProviderSetup setup, Func<IServiceProvider, I> factory)
    {
        var instance = default(I);
        var sp = setup.BuildServiceProvider();
        setup.TryRegister(typeof(I), () => instance ?? (instance = factory(sp)));
        return setup;
    }

    public static IProviderSetup TryAddSingleton<I>(this IProviderSetup setup, Func<I> factory)
        => setup.TryAddSingleton(_ => factory());

    public static IProviderSetup TryAddSingleton<I, T>(this IProviderSetup setup) where T : class, I, new()
        => setup.TryAddSingleton(_ => (I)(object)new T());
}