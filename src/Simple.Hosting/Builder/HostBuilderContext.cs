using Simple.Configuration;
using Simple.DI;

namespace Simple.Hosting;

public class HostBuilderContext(IConfiguration config, IHostingEnvironment hostingEnv, IProviderSetup setup)
{
    /// <summary> The <see cref="IConfiguration" /> containing the merged configuration of the application and the <see cref="IHost" /> </summary>
    public IConfiguration Configuration { get; set; } = config;

    /// <summary> The <see cref="IHostingEnvironment" /> initialized by the <see cref="IHost" /> </summary>
    public IHostingEnvironment HostingEnvironment => hostingEnv;

    /// <summary> DI container setup </summary>
    public IProviderSetup ProviderSetup => setup;
}