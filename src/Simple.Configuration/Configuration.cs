using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace Simple.Configuration
{
    /// <inheritdoc />
    internal class Configuration : Dictionary<string, object>, IConfiguration
    {
        public Configuration() : base(StringComparer.OrdinalIgnoreCase)
        {
            Json = new();
        }

        /// <inheritdoc />
        public JObject Json { get; }
    }
}
