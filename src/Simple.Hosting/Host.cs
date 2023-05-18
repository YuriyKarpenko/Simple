using System;

using Simple.Configuration;
using Simple.Logging;

namespace Simple.Hosting
{
    public class Host : IHost
    {
        public static IHostBuilder CreateDefaultBuilder(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config.AddEnvironmentVariables()
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true);

                    //if (args != null)
                    //{
                    //    config.AddCommandLine(args);
                    //}
                })
                .ConfigureLogging((hostingContext, logging) =>
                    logging.AddConfiguration(hostingContext.Configuration.Json.GetValue("Logging"))
                        .AddConsole()
                        .AddDebug()
                );

            return builder;
        }


        protected ILogger _logger;

        public Host(HostBuilderContext context)
        {
            Services = context.Services.BuildServiceProvider();
            _logger = Services.GetLogger(GetType());
        }

        public virtual IServiceProvider Services { get; }

        public virtual void Start(Action entryPoint)
        {
            _logger.Debug("Starting");
            entryPoint();
        }

        public void Dispose()
        {
            (Services as IDisposable)?.Dispose();
        }
    }
}
