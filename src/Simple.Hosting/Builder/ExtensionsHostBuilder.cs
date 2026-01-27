using Simple.DI;
using Simple.Logging.Configuration;

namespace Simple.Hosting;

public static class ExtensionsHostBuilder
{
    /// <summary>
    /// Adds a delegate for configuring the provided <see cref="LoggerFactory"/>. This may be called multiple times.
    /// </summary>
    /// <param name="hostBuilder">The <see cref="IWebHostBuilder" /> to configure.</param>
    /// <param name="configureLogging">The delegate that configures the <see cref="LoggerFactory"/>.</param>
    /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
    public static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder, Action<HostBuilderContext, ILogOptions> configureLogging)
    {
        return hostBuilder.ConfigureServices(c => c.ProviderSetup.AddLogging(builder => configureLogging(c, builder)));
    }

    public static IHostBuilder UseHostedService<THostedService>(this IHostBuilder hostBuilder, Func<IServiceProvider, THostedService> factory) where THostedService : class, IHostedService
    {
        return hostBuilder.ConfigureServices(c => c.ProviderSetup.AddSingleton<IHostedService>(factory));
    }

}