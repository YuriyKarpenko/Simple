using System.Collections.Generic;

using Simple.Configuration.Sources;

namespace Simple.Configuration
{
    /// <summary>
    /// Represents a type used to build application configuration.
    /// </summary>
    public interface IConfigurationBuilder
    {
        /// <summary>
        /// Gets a key/value collection that can be used to share data between the <see cref="IConfigurationBuilder"/>
        /// and the registered <see cref="IConfigurationSource"/>s.
        /// </summary>
        IDictionary<string, object> Properties { get; }

        /// <summary>
        /// Gets the sources used to obtain configuration values
        /// </summary>
        IList<IConfigurationSource> Sources { get; }

        /// <summary>
        /// Builds an <see cref="IConfiguration"/> with keys and values from the set of sources registered in
        /// <see cref="Sources"/>.
        /// </summary>
        /// <returns>An <see cref="IConfigurationRoot"/> with keys and values from the registered sources.</returns>
        IConfiguration Build();
    }
}
