using System;
using System.Collections.Generic;
using System.Reflection;

using Simple.Configuration;
using Simple.DI;
using Simple.Logging;

namespace Simple.Hosting
{
    public class HostBuilder : IHostBuilder
    {
        private readonly List<Action<HostBuilderContext, IProviderSetup>> _configureServicesDelegates = new();
        private readonly List<Action<HostBuilderContext, IConfigurationBuilder>> _configureAppConfigurationBuilderDelegates = new();

        private bool _webHostBuilt;

        #region configure builder

        /// <inheritdoc />
        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IProviderSetup> configureServices)
        {
            _configureServicesDelegates.Add(Throw.IsArgumentNullException(configureServices, nameof(configureServices)));
            return this;
        }

        /// <inheritdoc />
        public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _configureAppConfigurationBuilderDelegates.Add(Throw.IsArgumentNullException(configureDelegate, nameof(configureDelegate)));
            return this;
        }

        #endregion

        /// <inheritdoc />
        public IHost Build()
        {
            if (_webHostBuilt)
            {
                Throw.Exception(new InvalidOperationException("HostBuilder build again"));
            }
            _webHostBuilt = true;

            var context = BuildCommonServices();

            return new Host(context);
        }


        protected virtual HostBuilderContext BuildCommonServices()
        {
            var _config = new ConfigurationBuilder()
                .AddEnvironmentVariables("ASPNETCORE_")
                .Build();

            var hostingEnv = new HostingEnvironment(
                envName: _config.Get<string>(EnvKey.Environment) ?? "Release",
                appName: Assembly.GetEntryAssembly().GetName().Name, //?? _config.Get<string>(EnvKey.ApplicationName),
                rootPath: ResolveContentRootPath(_config, AppContext.BaseDirectory));

            var services = Locator.Setup();
            services.AddConst<IHostingEnvironment>(hostingEnv);

            var context = new HostBuilderContext(_config, hostingEnv, services);
            services.AddConst(context);

            #region init configuration

            var builder = new ConfigurationBuilder()
                .SetBasePath(context.HostingEnvironment.ContentRootPath)
                .AddConfiguration(_config);

            foreach (var configureAppConfiguration in _configureAppConfigurationBuilderDelegates)
            {
                configureAppConfiguration(context, builder);
            }

            var configuration = builder.Build();
            services.AddConst<IConfiguration>(configuration);
            context.Configuration = configuration;

            #endregion

            services.AddLogging();

            foreach (var configureServices in _configureServicesDelegates)
            {
                configureServices(context, services);
            }

            return context;
        }


        protected virtual string ResolveContentRootPath(IConfiguration config, string basePath)
        {
            return basePath;
        }
    }
}
