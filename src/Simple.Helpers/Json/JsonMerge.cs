using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text.Json;

namespace Simple.Helpers;

#if NET5_0_OR_GREATER
public static class JsonMerge
{
    /// <summary> Hard cap for recursive object merges; guards against stack overflow on deep trees. </summary>
    private const int MaxRecursionDepth = 64;

    /// <summary> Global default options; pass per-call options via <c>jsonMergeOptions</c> instead of mutating this. </summary>
    public static readonly JsonMergeOptions MergeOptions = JsonMergeOptions.Default;

    public static JsonElement MergeFast(ref JsonElement master, JsonElement addon, JsonSerializerOptions? options = null, JsonMergeOptions? jsonMergeOptions = null)
    {
        if (master.ValueKind != JsonValueKind.Object)
        {
            Throw.Exception(new JsonException($"MergeFast: master must be an object, but was {master.ValueKind}."));
        }

        if (addon.ValueKind != JsonValueKind.Object)
        {
            Throw.Exception(new JsonException($"MergeFast: addon must be an object, but was {addon.ValueKind}."));
        }

        options ??= JsonConvertMs.JsonOptions;

        //  initial capacity: saves the first ArrayBufferWriter reallocation (0 → geometric growth)
        var buffer = new ArrayBufferWriter<byte>(256);
        var comparer = options.ToNode().PropertyNameCaseInsensitive
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

        using (var writer = new Utf8JsonWriter(buffer, options.ToWrite()))
        {
            WriteMergedObject(writer, master, addon, jsonMergeOptions ?? MergeOptions, comparer, 0);
        }

        var reader = new Utf8JsonReader(buffer.WrittenSpan, options.ToRead());
        master = JsonElement.ParseValue(ref reader);
        return master;
    }

    public static JsonElement MergeFast(string? masterJson, string? addonJson, JsonSerializerOptions? options = null, JsonMergeOptions? jsonMergeOptions = null)
    {
        options ??= JsonConvertMs.JsonOptions;

        var masterIsEmpty = string.IsNullOrEmpty(masterJson);
        var addonIsEmpty  = string.IsNullOrEmpty(addonJson);

        if (masterIsEmpty && addonIsEmpty)
        {
            using var emptyDoc = JsonDocument.Parse("{}");
            return emptyDoc.RootElement.Clone();
        }

        var addonEl = addonIsEmpty  ? default : JsonConvertMs.ParseEl(addonJson, options.ToDoc());
        if (masterIsEmpty)
        {
            return addonEl;
        }

        var masterEl = JsonConvertMs.ParseEl(masterJson, options.ToDoc());
        if (!addonIsEmpty)
        {
            MergeFast(ref masterEl, addonEl, options, jsonMergeOptions);
        }

        return masterEl;
    }


    private static void WriteMergedObject(
        Utf8JsonWriter writer,
        JsonElement obj1,
        JsonElement obj2,
        JsonMergeOptions opt,
        StringComparer comparer,
        int depth)
    {
        if (depth >= MaxRecursionDepth)
        {
            Throw.Exception(new JsonException($"MergeFast: maximum depth of {MaxRecursionDepth} exceeded."));
        }

        writer.WriteStartObject();

        // ⚡ ускоряем lookup (убираем O(n²)); дубль-ключи в addon: last-wins (как JsonObject indexer / JS)
        var dict2 = new Dictionary<string, JsonElement>(comparer);
        foreach (var p in obj2.EnumerateObject())
        {
            dict2[p.Name] = p.Value;
        }

        //  дубль-ключи в master: пишутся оба вхождения (валидный JSON; при чтении обратно last-wins)

        // 1. properties from obj1
        foreach (var p1 in obj1.EnumerateObject())
        {
            if (!dict2.TryGetValue(p1.Name, out var v2))
            {
                // not in the second one → write as-is
                writer.WritePropertyName(p1.Name);
                p1.Value.WriteTo(writer);
                continue;
            }

            // in both → merge

            if (v2.ValueKind == JsonValueKind.Null)
            {
                // write null explicitly
                if (opt.KeepNullValue)
                {
                    writer.WritePropertyName(p1.Name);
                    v2.WriteTo(writer);
                }
                dict2.Remove(p1.Name);
                continue;
            }

            var v1 = p1.Value;
            writer.WritePropertyName(p1.Name);

            if (v1.ValueKind == JsonValueKind.Object && v2.ValueKind == JsonValueKind.Object)
            {
                //  recursive deep merge
                HandleObject(writer, v1, v2, opt, comparer, depth + 1);
            }
            else if (v1.ValueKind == JsonValueKind.Array && v2.ValueKind == JsonValueKind.Array)
            {
                //  recursive deep merge
                HandleArray(writer, v1, v2, opt);
            }
            else
            {
                // replace
                v2.WriteTo(writer);
            }

            dict2.Remove(p1.Name); //  already written — do not duplicate below
        }

        // 2. remaining (new) properties from obj2
        foreach (var kv in dict2)
        {
            if (kv.Value.ValueKind == JsonValueKind.Null && !opt.KeepNullValue)
            {
                continue;
            }

            writer.WritePropertyName(kv.Key);
            kv.Value.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    private static void HandleObject(Utf8JsonWriter writer, JsonElement v1, JsonElement v2, JsonMergeOptions opt, StringComparer comparer, int depth)
    {
        switch (opt.ObjectBehavior)
        {
            case MergeBehavior.Ignore:
                v1.WriteTo(writer);
                break;

            case MergeBehavior.Replace:
                v2.WriteTo(writer);
                break;

            case MergeBehavior.Merge:
                WriteMergedObject(writer, v1, v2, opt, comparer, depth);
                break;

            default:
                Throw.Exception(new ArgumentOutOfRangeException(nameof(opt.ObjectBehavior), opt.ObjectBehavior, "MergeFast: unknown ObjectBehavior."));
                break;
        }
    }

    private static void HandleArray(Utf8JsonWriter writer, JsonElement v1, JsonElement v2, JsonMergeOptions opt)
    {
        switch (opt.ArrayBehavior)
        {
            case MergeBehavior.Ignore:
                v1.WriteTo(writer);
                break;

            case MergeBehavior.Replace:
                v2.WriteTo(writer);
                break;

            case MergeBehavior.Merge:
                writer.WriteStartArray();

                foreach (var i in v1.EnumerateArray())
                {
                    i.WriteTo(writer);
                }

                foreach (var i in v2.EnumerateArray())
                {
                    i.WriteTo(writer);
                }

                writer.WriteEndArray();
                break;

            default:
                Throw.Exception(new ArgumentOutOfRangeException(nameof(opt.ArrayBehavior), opt.ArrayBehavior, "MergeFast: unknown ArrayBehavior."));
                break;
        }
    }
}
#endif