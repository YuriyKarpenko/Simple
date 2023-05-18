using Simple.Configuration;
using Simple.DI;

namespace Simple.Hosting
{
    public class HostBuilderContext
    {
        public HostBuilderContext(IConfiguration config, IHostingEnvironment hostingEnv, IProviderSetup services)
        {
            Configuration = config;
            HostingEnvironment = hostingEnv;
            Services = services;
        }


        /// <summary> The <see cref="IConfiguration" /> containing the merged configuration of the application and the <see cref="IHost" /> </summary>
        public IConfiguration Configuration { get; set; }

        /// <summary> The <see cref="IHostingEnvironment" /> initialized by the <see cref="IHost" /> </summary>
        public IHostingEnvironment HostingEnvironment { get; }
        public IProviderSetup Services { get; }
    }
}
