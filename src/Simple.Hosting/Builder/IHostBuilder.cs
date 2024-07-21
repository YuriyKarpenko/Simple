using System.Reflection;

using Simple.Configuration;
using Simple.DI;

namespace Simple.Hosting;

/**
 * настраивает HostBuilderContext(IConfiguration, IHostingEnvironment, IProviderSetup) :
 * 1 создает IProviderSetup
 * 2 настраивает и регистрирует IHostingEnvironment
 * 3 создает HostBuilderContext
 * 4 настраивает и регистрирует IConfiguration (HostBuilderContext.Config + настроенные)
 * 5 настраивает IProviderSetup
 */
public interface IHostBuilder
{
    /// <summary> Posible to adjust <see cref="HostingEnvironment"/> (on build runs 1st) </summary>
    /// <param name="configureHostEnvironment">The delegate for configuring the <see cref="HostingEnvironment"/> </param>
    /// <returns></returns>
    IHostBuilder ConfigureHostEnvironment(Action<HostingEnvironment> configureHostEnvironment);

    /// <summary> Adds a delegate for configuring the <see cref="IConfigurationBuilder"/> that will construct an <see cref="IConfiguration"/>. (on build runs 2nd) </summary>
    /// <param name="configureDelegate">The delegate for configuring the <see cref="IConfigurationBuilder" /> that will be used to construct an <see cref="IConfiguration" />.</param>
    /// <returns>The <see cref="IHostBuilder"/>.</returns>
    /// <remarks>
    /// The <see cref="IConfiguration"/> and <see cref="ILoggerFactory"/> on the <see cref="HostBuilderContext"/> are uninitialized at this stage.
    /// The <see cref="IConfigurationBuilder"/> is pre-populated with the settings of the <see cref="IHostBuilder"/>.
    /// </remarks>
    IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate);

    /// <summary> Adds a delegate for configuring additional services for the host or web application. This may be called multiple times. (on build runs 3th) </summary>
    /// <param name="configureServices">A delegate for configuring the <see cref="IProviderSetup"/>.</param>
    /// <returns>The <see cref="IHostBuilder"/>.</returns>
    IHostBuilder ConfigureServices(Action<HostBuilderContext> configureServices);

    /// <summary> Builds the required services and cofigurations for <see cref="IHost"/> </summary>
    IHost Build();
}

public class HostBuilder : IHostBuilder
{
    public static Assembly Assembly => Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

    private readonly List<Action<HostBuilderContext>> _configureServicesDelegates = new();
    private readonly List<Action<HostBuilderContext, IConfigurationBuilder>> _configureAppConfigurationBuilderDelegates = new();

    private Action<HostingEnvironment>? _configureHostEnvironment;
    private bool _webHostBuilt;

    #region configure builder

    /// <inheritdoc />
    public IHostBuilder ConfigureHostEnvironment(Action<HostingEnvironment> configureHostEnvironment)
    {
        _configureHostEnvironment = configureHostEnvironment;
        return this;
    }

    /// <inheritdoc />
    public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        _configureAppConfigurationBuilderDelegates.Add(Throw.IsArgumentNullException(configureDelegate, nameof(configureDelegate)));
        return this;
    }

    /// <inheritdoc />
    public IHostBuilder ConfigureServices(Action<HostBuilderContext> configureServices)
    {
        _configureServicesDelegates.Add(Throw.IsArgumentNullException(configureServices, nameof(configureServices)));
        return this;
    }

    #endregion

    /// <inheritdoc />
    public virtual IHost Build()
    {
        ChecBuild();
        var context = BuildCommonServices();

        return new Host(context);
    }


    protected void ChecBuild()
    {
        if (_webHostBuilt)
        {
            Throw.Exception(new InvalidOperationException("HostBuilder build again"));
        }
        _webHostBuilt = true;
    }

    protected virtual HostBuilderContext BuildCommonServices()
    {
        var services = Locator.Setup();

        #region create HostBuilderContext

        var builderConfig = new ConfigurationBuilder().AddEnvironmentVariables("ASPNETCORE_");
        var configTemp = builderConfig.Build();

        var asmName = Assembly.GetName();
        var hostingEnv = new HostingEnvironment(
            envName: configTemp.GetOrDefault<string>(EnvKey.Environment) ?? EnvironmentNames.Release,
            appName: configTemp.GetOrDefault<string>(EnvKey.ApplicationName) ?? asmName.Name ?? asmName.FullName,
            rootPath: ResolveContentRootPath(configTemp, AppContext.BaseDirectory));

        _configureHostEnvironment?.Invoke(hostingEnv);
        services.AddSingleton<IHostingEnvironment>(hostingEnv);

        var context = new HostBuilderContext(configTemp, hostingEnv, services);
        services.AddSingleton(context);

        #endregion

        #region init configuration

        builderConfig
            .SetBasePath(context.HostingEnvironment.ContentRootPath)
            //.AddJsonFile("config.json")
            //.AddJsonFile($"config.{hostingEnv.EnvironmentName}.json")
            .AddConfiguration(configTemp);

        foreach (var configureAppConfiguration in _configureAppConfigurationBuilderDelegates)
        {
            configureAppConfiguration(context, builderConfig);
        }

        var configuration = builderConfig.Build();
        services.AddConst<IConfiguration>(configuration);
        //  override configuration
        context.Configuration = configuration;

        #endregion

        services.AddLogging();

        foreach (var configureServices in _configureServicesDelegates)
        {
            configureServices(context);
        }

        return context;
    }


    protected virtual string ResolveContentRootPath(IConfiguration config, string basePath)
    {
        return basePath;
    }
}