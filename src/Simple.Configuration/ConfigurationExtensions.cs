using System;
using System.IO;

using Newtonsoft.Json;

using Simple.Configuration.Sources;
using Simple.Helpers;

namespace Simple.Configuration
{
    public static class ConfigurationExtensions
    {
        private static readonly string KeyBasePath = "BasePath";

        public static bool Bind(this IConfiguration c, object? o)
        {
            if (c != null && o != null)
            {
                var content = c.Json.ToString();
                JsonConvert.PopulateObject(content, o);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Adds an <see cref="IConfigurationSource"/> that reads configuration values from environment variables
        /// with a specified prefix.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="prefix">The prefix that environment variable names must start with. The prefix will be removed from the environment variable names.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddEnvironmentVariables(this IConfigurationBuilder builder, string? prefix = null)
            => builder.Add(new EnvironmentVariablesSource(prefix));

        /// <summary> Sets the FileProvider for file-based providers to a PhysicalFileProvider with the base path. </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="basePath">The absolute path of file-based providers.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder SetBasePath(this IConfigurationBuilder builder, string basePath)
        {
            builder.Ensure().Properties[KeyBasePath] = Throw.IsArgumentNullException(basePath, i => !string.IsNullOrEmpty(i), nameof(basePath));

            return builder;
        }

        public static string GetBasePath(this IConfigurationBuilder builder)
        {
            return builder.Ensure().Properties.TryGetValue(KeyBasePath, out var basePath)
                ? (string)basePath
                : AppContext.BaseDirectory;
        }

        /// <summary> Adds an existing configuration to <paramref name="builder"/>. </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="config">The <see cref="IConfiguration"/> to add.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddConfiguration(this IConfigurationBuilder builder, IConfiguration config)
            => builder.Add(new ChainedSource(Throw.IsArgumentNullException(config, nameof(config))));

        /// <summary> Adds a new configuration source. </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configureSource">Configures the source secrets.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder Add(this IConfigurationBuilder builder, IConfigurationSource configureSource)
        {
            Throw.IsArgumentNullException(configureSource, nameof(configureSource));
            builder.Ensure().Sources.Add(configureSource);
            return builder;
        }

        /// <summary>
        /// Adds a JSON configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="fileOrPath">Path relative to the base path or absolute path </param> 
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <param name="optional">Whether the file is optional.</param>
        /// <param name="reloadOnChange">Whether the configuration should be reloaded if the file changes.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddJsonFile(this IConfigurationBuilder builder, string fileOrPath, bool optional = false)
        {
            Throw.IsArgumentNullException(fileOrPath, i => !string.IsNullOrEmpty(i), nameof(fileOrPath));
            var (file, path) = fileOrPath == Path.GetFileName(fileOrPath)
                ? (fileOrPath, default(string))
                : (null, fileOrPath);
            return builder.Add(new JsonSource
            {
                FileName = file,
                FullPath = path,
                IsOptional = optional
            });
        }


        public static IConfigurationBuilder Ensure(this IConfigurationBuilder builder)
            => Throw.IsArgumentNullException(builder, nameof(builder));
    }
}