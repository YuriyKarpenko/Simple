using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace Simple.Configuration
{
    /// <summary> All options of host </summary>
    public interface IConfiguration : IDictionary<string, object>
    {
        /// <summary> Json data of configuration </summary>
        JObject Json { get; }
    }
}
