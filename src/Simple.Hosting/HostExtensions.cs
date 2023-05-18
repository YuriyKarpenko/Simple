using System;

using Simple.Logging;
using Simple.Logging.Configuration;

namespace Simple.Hosting
{
    public static class HostExtensions
    {
        /// <summary>
        /// Adds a delegate for configuring the provided <see cref="LoggerFactory"/>. This may be called multiple times.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IWebHostBuilder" /> to configure.</param>
        /// <param name="configureLogging">The delegate that configures the <see cref="LoggerFactory"/>.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        public static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder, Action<HostBuilderContext, ILogOptions> configureLogging)
        {
            return hostBuilder.ConfigureServices((context, collection) => collection.AddLogging(builder => configureLogging(context, builder)));
        }


        /// <summary>
        /// Checks if the current hosting environment name is <see cref="EnvironmentName.Development"/>.
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <returns>True if the environment name is <see cref="EnvironmentName.Development"/>, otherwise false.</returns>
        public static bool IsDevelopment(this IHostingEnvironment hostingEnvironment)
            => string.Equals(hostingEnvironment.Ensure().EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Checks if the current hosting environment name is <see cref="EnvironmentName.Production"/>.
        /// </summary>
        /// <param name="hostingEnvironment">An instance of <see cref="IHostingEnvironment"/>.</param>
        /// <returns>True if the environment name is <see cref="EnvironmentName.Production"/>, otherwise false.</returns>
        public static bool IsProduction(this IHostingEnvironment hostingEnvironment)
            => string.Equals(hostingEnvironment.Ensure().EnvironmentName, "Production", StringComparison.OrdinalIgnoreCase);


        private static IHostingEnvironment Ensure(this IHostingEnvironment env)
            => Throw.IsArgumentNullException(env, nameof(env));
    }
}