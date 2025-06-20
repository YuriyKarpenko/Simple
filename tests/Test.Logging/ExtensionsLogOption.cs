using System.Collections.Generic;

using Newtonsoft.Json.Linq;

using Simple.Logging.Configuration;

namespace Test.Logging;

public static class ExtensionsLogOption
{
    public static void Populate(this ILogOptionItem item, JObject jo)
    {
        if (jo.TryGetValue(nameof(LogOptionItem.LogLevel), out var jt))
        {
            Newtonsoft.Json.JsonConvert.PopulateObject(jt.ToString(), item.LogLevel);
            jo.Remove(nameof(LogOptionItem.LogLevel));
        }

        Newtonsoft.Json.JsonConvert.PopulateObject(jo.ToString(), item);
    }

    public static void Populate(this LogOptions item, JObject jo)
    {
        if (jo.TryGetValue(nameof(LogOptionItem.LogLevel), out var jt))
        {
            Newtonsoft.Json.JsonConvert.PopulateObject(jt.ToString(), item.LogLevel);
            jo.Remove(nameof(LogOptionItem.LogLevel));
        }

        var d = jo.ToObject<Dictionary<string, JObject>>();
        foreach (var k in d!.Keys)
        {
            var v = new LogOptionItem(k, null);
            item.EnsureOptionItem(v).Populate(d[k]);
        }
    }
}