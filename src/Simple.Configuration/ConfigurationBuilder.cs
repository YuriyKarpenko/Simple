using System;
using System.Collections.Generic;

using Simple.Configuration.Sources;

namespace Simple.Configuration
{
    /// <summary>
    /// Used to build key/value based configuration settings for use in an application.
    /// </summary>
    public class ConfigurationBuilder : IConfigurationBuilder
    {
        /// <inheritdoc />
        public IList<IConfigurationSource> Sources { get; } = new List<IConfigurationSource>();

        /// <inheritdoc />
        public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc />
        public IConfiguration Build()
        {
            var config = new Configuration();
            foreach (var source in Sources)
            {
                source.Build(this, config);
            }

            config.Merge(Properties);

            return config;
        }
    }
}