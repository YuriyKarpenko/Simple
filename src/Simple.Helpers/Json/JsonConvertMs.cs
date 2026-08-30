using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

// ReSharper disable MemberCanBePrivate.Global

namespace Simple.Helpers;

public static class JsonConvertMs
{
    /// <summary> Hard cap for recursive merge/diff walks; guards against stack overflow on deep trees. </summary>
    internal const int MaxRecursionDepth = 64;

    /// <summary>
    /// Global options used by the obsolete overloads.
    /// <para>
    /// Frozen via <see cref="JsonSerializerOptions.MakeReadOnly"/> at type initialization:
    /// any mutation (<c>Converters.Add</c>, property assignment, ...) throws
    /// <see cref="InvalidOperationException"/> at the mutation call site — not later,
    /// inside the first serialization somewhere deep in the call stack.
    /// </para>
    /// <para>
    /// Need custom converters? Use <see cref="CreateOptions"/> and pass the result explicitly.
    /// </para>
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerOptions.Default)
    {
        // AllowTrailingCommas = true,
        Converters =
        {
            // new System.Text.Json.Serialization.JsonStringEnumConverter(),
        },
        // no escaping: characters are written as-is
        Encoder              = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = null,
        // PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Skip,
        // WriteIndented               = true,
    };

    static JsonConvertMs()
    {
        // JsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

        //  freeze right away: a late mutation throws here, at the mutation call site,
        //  not at the first serializer use somewhere deep in the stack
        JsonOptions.MakeReadOnly();
    }

    /// <summary>
    /// Returns a fresh mutable copy of <see cref="JsonOptions"/> —
    /// add custom converters there and pass it explicitly to Serialize/Deserialize.
    /// </summary>
    public static JsonSerializerOptions CreateOptions()
        => new(JsonOptions);

    public static T? DeserializeObject<T>(ReadOnlySpan<byte> utf8Json, JsonSerializerOptions options)
        => utf8Json.IsEmpty ? default : JsonSerializer.Deserialize<T>(utf8Json, options);

    public static T? DeserializeObject<T>(ReadOnlySpan<char> json, JsonSerializerOptions options)
        => json.IsEmpty ? default : JsonSerializer.Deserialize<T>(json, options);

    //  whitespace-only strings go to the parser and throw JsonException
    public static T? DeserializeObject<T>(string? json, JsonSerializerOptions options)
        => string.IsNullOrEmpty(json) ? default : DeserializeObject<T>(json.AsSpan(), options);

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
            : Throw.Exception<JsonObject>(new JsonException($"CreateObject: expected a JSON object, but was {element.ValueKind}."));

    [Obsolete("Use options explicitly instead")]
    public static JsonObject CreateObject(JsonElement element)
        => CreateObject(element, JsonOptions.ToNode());


    public static JsonObject FromObject<T>(T? o, JsonSerializerOptions options)
        => o is null
            ? new JsonObject(options.ToNode())
            : AsObject(JsonSerializer.SerializeToNode(o, options), nameof(FromObject));

    [Obsolete("Use options explicitly instead")]
    public static JsonObject FromObject<T>(T? o)
        => FromObject(o, JsonOptions);

    #region parsing

    //  doc
    public static JsonDocument ParseDoc(ReadOnlyMemory<byte> utf8Json, JsonDocumentOptions options)
        => JsonDocument.Parse(utf8Json, options);

    public static JsonDocument ParseDoc(ReadOnlyMemory<char> json, JsonDocumentOptions options)
        => JsonDocument.Parse(json, options);

    public static JsonDocument ParseDoc(string? json, JsonDocumentOptions options)
        => json is null
            ? Throw.Exception<JsonDocument>(new ArgumentNullException(nameof(json)))
            : ParseDoc(json.AsMemory(), options);

    public static JsonDocument ParseDoc(string? json)
        => ParseDoc(json, JsonOptions.ToDoc());

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
            : AsObject(JsonNode.Parse(utf8Json, options.ToNode(), options.ToDoc()), nameof(ParseObj));

    public static JsonObject ParseObj(string? json, JsonSerializerOptions options)
        => string.IsNullOrEmpty(json)
            ? new JsonObject(options.ToNode())
            : AsObject(JsonNode.Parse(json!, options.ToNode(), options.ToDoc()), nameof(ParseObj));

    private static JsonObject AsObject(JsonNode? node, string method)
        => node is JsonObject obj
            ? obj
            : Throw.Exception<JsonObject>(new JsonException($"{method}: expected a JSON object, but was {(node is null ? JsonValueKind.Null : node.GetValueKind())}."));

    [Obsolete("Use options explicitly instead")]
    public static JsonObject ParseObj(string? json)
        => ParseObj(json, JsonOptions);

    #endregion

    #region merge

    [Obsolete("Use JsonMerge.MergeFast() instead")]
    public static void Merge(JsonObject master, JsonObject addon)
    {
        Throw.IsArgumentNullException(master, nameof(master));
        Throw.IsArgumentNullException(addon, nameof(addon));

        if (ReferenceEquals(master, addon))
        {
            return; //  merging an object into itself is a no-op
        }

        MergeCore(master, addon, 0);
    }

    private static void MergeCore(JsonObject master, JsonObject addon, int depth)
    {
        if (depth >= MaxRecursionDepth)
        {
            Throw.Exception(new JsonException($"Merge: maximum depth of {MaxRecursionDepth} exceeded."));
        }

        foreach (var kv in addon)
        {
            if (master[kv.Key] is JsonObject mObj && kv.Value is JsonObject aObj)
            {
                MergeCore(mObj, aObj, depth + 1);
            }
            else
            {
                // Replace (like MergeArrayHandling.Replace)
                master[kv.Key] = kv.Value?.DeepClone();
            }
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
            Encoder  = options.Encoder,
            Indented = options.WriteIndented,
            MaxDepth = options.MaxDepth,
            //  SkipValidation stays false (default): the writer validates the output structure —
            //  broken JSON is worse than the micro-perf cost of validation
        };


#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    public static JsonObject FindDiff(this JsonNode? jCurrent, JsonNode? jOld, JsonNodeOptions? options = null)
        => FindDiffCore(jCurrent, jOld, options ?? JsonConvertMs.JsonOptions.ToNode(), 0);

    private static JsonObject FindDiffCore(JsonNode? jCurrent, JsonNode? jOld, JsonNodeOptions nOpt, int depth)
    {
        if (depth >= JsonConvertMs.MaxRecursionDepth)
        {
            Throw.Exception(new JsonException($"FindDiff: maximum depth of {JsonConvertMs.MaxRecursionDepth} exceeded."));
        }

        var diff = new JsonObject(nOpt);
        if (JsonNode.DeepEquals(jCurrent, jOld))
        {
            return diff;
        }

        if (jCurrent is JsonObject oCurrent && jOld is JsonObject oOld)
        {
            //  name case-handling follows the node option: with PropertyNameCaseInsensitive
            //  "Key" and "key" are the same key for both the serializer and the diff
            var nameComparer = nOpt.PropertyNameCaseInsensitive
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal;

            var namesCurrent = oCurrent.Select(p => p.Key).ToHashSet(nameComparer);
            var namesOld     = oOld.Select(p => p.Key).ToHashSet(nameComparer);

            foreach (var k in namesOld.Except(namesCurrent, nameComparer))
            {
                diff[k] = new JsonObject(nOpt) { ["-"] = oOld[k]?.DeepClone() };
            }

            var added = namesCurrent.Except(namesOld, nameComparer).ToArray();
            foreach (var k in added)
            {
                diff[k] = new JsonObject(nOpt) { ["+"] = oCurrent[k]?.DeepClone() };
            }

            var unchanged = namesCurrent.Where(k => JsonNode.DeepEquals(oCurrent[k], oOld[k])).ToArray();
            var modified  = namesCurrent.Except(added, nameComparer).Except(unchanged, nameComparer);

            foreach (var k in modified)
            {
                var d = FindDiffCore(oCurrent[k], oOld[k], nOpt, depth + 1);
                if (d.Count > 0)
                {
                    diff[k] = d;
                }
            }
        }
        else if (jCurrent is JsonArray aCurrent && jOld is JsonArray aOld)
        {
            //  presence semantics (like before), but O(n+m) via JSON-string keys instead of O(n·m) DeepEquals scan;
            //  caveat: equal objects with different property order now count as different (keys are serialized forms)
            var currentItems = aCurrent.Where(n => n != null).Select(n => (Node: n!, Key: n!.ToJsonString())).ToArray();
            var oldItems     = aOld.Where(n => n     != null).Select(n => (Node: n!, Key: n!.ToJsonString())).ToArray();

            var currentKeys = currentItems.Select(i => i.Key).ToHashSet(StringComparer.Ordinal);
            var oldKeys     = oldItems.Select(i => i.Key).ToHashSet(StringComparer.Ordinal);

            var minus = oldItems.Where(i => !currentKeys.Contains(i.Key)).Select(i => i.Node).ToArray();
            if (minus.Length > 0)
            {
                diff["-"] = new JsonArray(nOpt, minus.Select(i => i.DeepClone()).ToArray());
            }

            var plus = currentItems.Where(i => !oldKeys.Contains(i.Key)).Select(i => i.Node).ToArray();
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