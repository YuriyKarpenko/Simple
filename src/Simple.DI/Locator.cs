using System;

namespace Simple.DI;

/// <summary> Application-wide service locator over the shared <see cref="Resolver" /> </summary>
public class Locator
{
    private static readonly Resolver _instance = new();

    /// <summary> Gets the <see cref="IServiceProvider" /> that provides access to the application's service container </summary>
    public static IServiceProvider Current => _instance;

    /// <summary> Allows registering the application's service factory <see cref="IProviderSetup" /> </summary>
    public static IProviderSetup Setup() => _instance;

    /// <summary> Allows registering the application's service factory <see cref="IProviderSetup" /> </summary>
    public static IServiceProvider Setup(Action<IProviderSetup> act)
    {
        if (act == null)
        {
            throw new ArgumentNullException(nameof(act));
        }

        act(_instance);
        return _instance;
    }
}