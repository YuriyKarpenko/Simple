using System;
using System.IO;

namespace Simple.Configuration.Sources
{
    /// <summary> Represents a base class for file based <see cref="IConfigurationSource"/>. </summary>
    public abstract class FileSourceBase : IConfigurationSource
    {
        #region IConfigurationSource

        /// <summary> Builds the <see cref="IConfigurationSource"/> for this source. </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>A <see cref="IConfigurationSource"/></returns>
        public abstract IConfiguration Build(IConfigurationBuilder builder, IConfiguration config);

        #endregion

        public string? FileName { get; set; }

        /// <summary> The path to the file. </summary>
        public string? FullPath { get; set; }

        /// <summary> Determines if loading the file is optional. </summary>
        public bool IsOptional { get; set; }

        /// <summary> Will be called if an uncaught exception occurs in FileConfigurationProvider.Load. </summary>
        public Action<Exception>? OnLoadException { get; set; }

        /// <summary> Called to use any default settings on the builder like the FileProvider or FileLoadExceptionHandler. </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        public void EnsureDefaults(IConfigurationBuilder builder)
        {
            if (string.IsNullOrEmpty(FullPath) && !string.IsNullOrEmpty(FileName))
            {
                FullPath = Path.Combine(builder.GetBasePath(), FileName);
            }
        }
    }
}