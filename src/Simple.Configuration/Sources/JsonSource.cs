using System;
using System.IO;

using Newtonsoft.Json.Linq;

namespace Simple.Configuration.Sources
{
    /// <summary> Represents a JSON file as an <see cref="IConfigurationSource"/>. </summary>
    public class JsonSource : FileSourceBase
    {
        #region IConfigurationSource

        /// <inheritdoc />
        public override IConfiguration Build(IConfigurationBuilder builder, IConfiguration config)
        {
            EnsureDefaults(builder);

            var content = Load();
            if (!string.IsNullOrWhiteSpace(content))
            {
                var o = JObject.Parse(content!);
                config.Json.Merge(o, null);
            }

            return config;
        }

        #endregion

        private string? Load()
        {
            if (FullPath == null || !File.Exists(FullPath))
            {
                if (!IsOptional) // Always optional on reload
                {
                    Throw.Exception(new FileNotFoundException($"The configuration file '{FullPath}' was not found and is not optional."));
                }

                return null;
            }

            try
            {
                return File.ReadAllText(FullPath!);
            }
            catch (Exception e)
            {
                if (OnLoadException == null)
                {
                    throw;
                }
                else
                {
                    OnLoadException.Invoke(e);
                }
            }

            return null;
        }
    }
}