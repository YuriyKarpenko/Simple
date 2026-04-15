using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

// ReSharper disable MemberCanBePrivate.Global

namespace Simple.Helpers;

public class JsonConvertMs
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerOptions.Default)
    {
        // AllowTrailingCommas = true,
        Converters =
        {
            // new System.Text.Json.Serialization.JsonStringEnumConverter(),
        },
        // Отключает экранирование, оставляя символы как есть
        Encoder              = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = null,
        // PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Skip,
        // WriteIndented               = true,
    };

    static JsonConvertMs()
    {
        // JsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    }

    public static T? DeserializeObject<T>(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions options)
        => utf8Json.IsEmpty ? default : JsonSerializer.Deserialize<T>(utf8Json, options);

    public static T? DeserializeObject<T>(ReadOnlySpan<char> json, JsonSerializerOptions options)
        => json.IsEmpty ? default : JsonSerializer.Deserialize<T>(json, options);

    public static T? DeserializeObject<T>(string? json, JsonSerializerOptions options)
        => string.IsNullOrWhiteSpace(json) ? default : DeserializeObject<T>(json.AsSpan(), options);

    [Obsolete("Use options explicitly instead")]
    public static T? DeserializeObject<T>(string? json)
        => DeserializeObject<T>(json, JsonOptions);

    public static T? DeserializeObject<T>(JsonNode? jo, JsonSerializerOptions options)
        => jo is null ? default : jo.Deserialize<T>(options);

    [Obsolete("Use options explicitly instead")]
    public static T? DeserializeObject<T>(JsonNode? jo)
        => DeserializeObject<T>(jo, JsonOptions);


    public static string? SerializeObject<T>(T? o, JsonSerializerOptions options)
        => o is null ? null : JsonSerializer.Serialize(o, options);

    [Obsolete("Use options explicitly instead")]
    public static string? SerializeObject<T>(T? o)
        => SerializeObject(o, JsonOptions);


    public static JsonObject CreateObject(JsonElement element, JsonNodeOptions options)
        => element.ValueKind == JsonValueKind.Object
            ? JsonObject.Create(element, options)!
            : new JsonObject(options);

    [Obsolete("Use options explicitly instead")]
    public static JsonObject CreateObject(JsonElement element)
        => CreateObject(element, JsonOptions.ToNode());


    public static JsonObject FromObject<T>(T? o, JsonSerializerOptions options)
        => o is null
            ? new JsonObject(options.ToNode())
            : JsonSerializer.SerializeToNode(o, options) as JsonObject ?? new JsonObject(options.ToNode());

    [Obsolete("Use options explicitly instead")]
    public static JsonObject FromObject<T>(T? o)
        => FromObject(o, JsonOptions);

    #region parsing

    //  doc
    public static JsonDocument ParseDoc(ReadOnlyMemory<byte> utf8Json, JsonDocumentOptions options)
        => JsonDocument.Parse(utf8Json, options);

    public static JsonDocument ParseDoc(ReadOnlyMemory<char> json, JsonDocumentOptions options)
        => JsonDocument.Parse(json, options);

    public static JsonDocument ParseDoc(string json, JsonDocumentOptions options)
        => ParseDoc(json.AsMemory(), options);

    [Obsolete("Use options explicitly instead")]
    public static JsonDocument ParseDoc(string json)
        => ParseDoc(json.AsMemory(), JsonOptions.ToDoc());

    //  element
    public static JsonElement ParseEl(ReadOnlyMemory<byte> utf8Json, JsonDocumentOptions options)
    {
        using var doc = ParseDoc(utf8Json, options);
        return doc.RootElement.Clone();
    }

    public static JsonElement ParseEl(ReadOnlyMemory<char> json, JsonDocumentOptions options)
    {
        using var doc = ParseDoc(json, options);
        return doc.RootElement.Clone();
    }

    public static JsonElement ParseEl(string? json, JsonDocumentOptions options)
        => string.IsNullOrEmpty(json) ? default : ParseEl(json.AsMemory(), options);

    [Obsolete("Use options explicitly instead")]
    public static JsonElement ParseEl(string? json)
        => ParseEl(json, JsonOptions.ToDoc());

    //  obj
    public static JsonObject ParseObj(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions options)
        => utf8Json.IsEmpty
            ? new JsonObject(options.ToNode())
            : JsonNode.Parse(utf8Json, options.ToNode(), options.ToDoc()) as JsonObject ?? new JsonObject(options.ToNode());

    public static JsonObject ParseObj(string? json, JsonSerializerOptions options)
        => string.IsNullOrEmpty(json)
            ? new JsonObject(options.ToNode())
            : JsonNode.Parse(json, options.ToNode(), options.ToDoc()) as JsonObject ?? new JsonObject(options.ToNode());

    // public static JsonObject ParseObj(string? json, JsonNodeOptions options)
    //     => string.IsNullOrEmpty(json)
    //         ? new JsonObject(options)
    //         : JsonNode.Parse(json, options) as JsonObject ?? new JsonObject(options);

    [Obsolete("Use options explicitly instead")]
    public static JsonObject ParseObj(string? json)
        => ParseObj(json, JsonOptions);

    #endregion

    #region merge

    [Obsolete("Use JsonMerge.MergeFast() instead")]
    public static void Merge(JsonObject master, JsonObject addon)
    {
        try
        {
            Throw.IsArgumentNullException(master, nameof(master));
            Throw.IsArgumentNullException(addon, nameof(addon));

            foreach (var kv in addon)
            {
                if (master[kv.Key] is JsonObject mObj && kv.Value is JsonObject aObj)
                {
                    Merge(mObj, aObj);
                }
                else
                {
                    // Replace (аналог MergeArrayHandling.Replace)
                    master[kv.Key] = kv.Value?.DeepClone();
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"tf {nameof(JsonConvertMs)}.{nameof(Merge)}(addon)", ex);
        }
    }

    [Obsolete("Use JsonMerge.MergeFast() instead")]
    public static void Merge(JsonObject master, string? json)
    {
        if (!string.IsNullOrEmpty(json))
        {
            Merge(master, ParseObj(json));
        }
    }

    #endregion

}

public static class ExtensionsSystemTextJson
{
    public static JsonDocumentOptions ToDoc(this JsonSerializerOptions options)
        => new()
        {
            AllowTrailingCommas = options.AllowTrailingCommas,
            CommentHandling     = options.ReadCommentHandling,
            MaxDepth            = options.MaxDepth,
        };

    public static JsonNodeOptions ToNode(this JsonSerializerOptions options)
        => new()
        {
            PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive,
        };

    public static JsonReaderOptions ToRead(this JsonSerializerOptions options)
        => new()
        {
            AllowTrailingCommas = options.AllowTrailingCommas,
            CommentHandling     = options.ReadCommentHandling,
            MaxDepth            = options.MaxDepth,
        };

    public static JsonWriterOptions ToWrite(this JsonSerializerOptions options)
        => new()
        {
            Encoder        = options.Encoder,
            Indented       = options.WriteIndented,
            MaxDepth       = options.MaxDepth,
            SkipValidation = true,
        };


#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    public static JsonObject FindDiff(this JsonNode? jCurrent, JsonNode? jOld, JsonNodeOptions? options = null)
    {
        var nOpt = options ?? JsonConvertMs.JsonOptions.ToNode();
        var diff = new JsonObject(nOpt);
        if (JsonNode.DeepEquals(jCurrent, jOld))
        {
            return diff;
        }

        if (jCurrent is JsonObject oCurrent && jOld is JsonObject oOld)
        {
            var namesCurrent = oCurrent.Select(p => p.Key).ToHashSet();
            var namesOld = oOld.Select(p => p.Key).ToHashSet();

            foreach (var k in namesOld.Except(namesCurrent))
            {
                diff[k] = new JsonObject(nOpt) { ["-"] = oOld[k]?.DeepClone() };
            }

            var added = namesCurrent.Except(namesOld).ToArray();
            foreach (var k in added)
            {
                diff[k] = new JsonObject(nOpt) { ["+"] = oCurrent[k]?.DeepClone() };
            }

            var unchanged = namesCurrent.Where(k => JsonNode.DeepEquals(oCurrent[k], oOld[k])).ToArray();
            var modified = namesCurrent.Except(added).Except(unchanged);

            foreach (var k in modified)
            {
                var d = FindDiff(oCurrent[k], oOld[k], nOpt);
                if (d.Count > 0)
                {
                    diff[k] = d;
                }
            }
        }
        else if (jCurrent is JsonArray aCurrent && jOld is JsonArray aOld)
        {
            var minus = aOld
                .Where(o => o != null && !aCurrent.Any(c => JsonNode.DeepEquals(c, o)))
                .Cast<JsonNode>().ToArray();
            if (minus.Length > 0)
            {
                diff["-"] = new JsonArray(nOpt, minus.Select(i => i.DeepClone()).ToArray());
            }

            var plus = aCurrent
                .Where(c => c != null && !aOld.Any(o => JsonNode.DeepEquals(c, o)))
                .Cast<JsonNode>().ToArray();
            if (plus.Length > 0)
            {
                diff["+"] = new JsonArray(nOpt, plus.Select(i => i.DeepClone()).ToArray());
            }
        }
        else
        {
            diff["-"] = jOld?.DeepClone();
            diff["+"] = jCurrent?.DeepClone();
        }

        return diff;
    }

#endif

    public static string[] PropertyNames(this JsonObject? jo)
        => jo?.Select(p => p.Key).ToArray() ?? [];
}