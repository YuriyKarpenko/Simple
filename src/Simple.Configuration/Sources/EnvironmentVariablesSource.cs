using System;
using System.Collections;
using System.Linq;

using Simple.Helpers;

namespace Simple.Configuration.Sources
{
    internal class EnvironmentVariablesSource : IConfigurationSource
    {
        private readonly string _prefix;

        public EnvironmentVariablesSource(string? prefix = null)
        {
            _prefix = prefix ?? string.Empty;
        }

        /// <inheritdoc />
        public IConfiguration Build(IConfigurationBuilder builder, IConfiguration config)
        {
            var dic = Environment.GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .Where(entry => ((string)entry.Key).StartsWith(_prefix, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(KeyConvert, i => i.Value);

            //var dic = Environment.GetEnvironmentVariables()
            //    .Cast<KeyValuePair<string, object?>>()
            //    .Where(entry => entry.Key.StartsWith(_prefix, StringComparison.OrdinalIgnoreCase))
            //    .ToDictionary(i => i.Key.Substring(_prefix.Length), i => i.Value);

            config.Merge(dic!);

            return config;
        }


        private string KeyConvert(DictionaryEntry de)
            => de.Key.ToString()!.Substring(_prefix.Length);

    }
}