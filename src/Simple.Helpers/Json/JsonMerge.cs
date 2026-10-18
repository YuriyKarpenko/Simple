using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

namespace Simple.Helpers;

public enum MergeBehavior
{
    /// <summary> Try to merge. </summary>
    Merge,

    /// <summary> Replace from addon. </summary>
    Replace,

    /// <summary> Leave as master. </summary>
    Ignore,
}

public struct JsonMergeOptions()
{
    public static readonly JsonMergeOptions Default = new();

    public MergeBehavior ArrayBehavior = MergeBehavior.Replace;

    /// <summary> false = exclude property, true = keep property (default = false). </summary>
    public bool KeepNullValue;

    public MergeBehavior ObjectBehavior;
}

#if NET5_0_OR_GREATER
public class JsonMerge
{
    public static JsonMergeOptions MergeOptions = JsonMergeOptions.Default;

    public static JsonElement MergeFast(ref JsonElement master, JsonElement addon, JsonSerializerOptions? options = null, JsonMergeOptions? jsonMergeOptions = null)
    {
        Debug.Assert(master.ValueKind == JsonValueKind.Object);
        Debug.Assert(addon.ValueKind  == JsonValueKind.Object);

        options ??= JsonConvertMs.JsonOptions;

        var buffer = new ArrayBufferWriter<byte>();
        var comparer = options.ToNode().PropertyNameCaseInsensitive
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

        using (var writer = new Utf8JsonWriter(buffer, options.ToWrite()))
        {
            WriteMergedObject(writer, master, addon, jsonMergeOptions ?? MergeOptions, comparer);
        }

        var reader = new Utf8JsonReader(buffer.WrittenSpan, options.ToRead());
        master = JsonElement.ParseValue(ref reader);
        return master;
    }

    public static JsonElement MergeFast(string? masterJson, string? addonJson, JsonSerializerOptions? options = null, JsonMergeOptions? jsonMergeOptions = null)
    {
        options ??= JsonConvertMs.JsonOptions;
        var masterEl = JsonConvertMs.ParseEl(masterJson, options.ToDoc());
        var addonEl = JsonConvertMs.ParseEl(addonJson, options.ToDoc());

        if (string.IsNullOrEmpty(masterJson))
        {
            return addonEl;
        }

        if (!string.IsNullOrEmpty(addonJson))
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
        StringComparer comparer)
    {
        writer.WriteStartObject();

        // ⚡ ускоряем lookup (убираем O(n²))
        var dict2 = new Dictionary<string, JsonElement>(comparer);
        foreach (var p in obj2.EnumerateObject())
        {
            dict2[p.Name] = p.Value;
        }

        // 1. свойства из obj1
        foreach (var p1 in obj1.EnumerateObject())
        {
            if (!dict2.TryGetValue(p1.Name, out var v2))
            {
                // нет во втором → пишем как есть
                writer.WritePropertyName(p1.Name);
                p1.Value.WriteTo(writer);
                continue;
            }

            // есть в обоих → merge

            if (v2.ValueKind == JsonValueKind.Null)
            {
                // null вставляем явно
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
                // 🔥 рекурсивный deep merge
                HandleObject(writer, v1, v2, opt, comparer);
            }
            else if (v1.ValueKind == JsonValueKind.Array && v2.ValueKind == JsonValueKind.Array)
            {
                // 🔥 рекурсивный deep merge
                HandleArray(writer, v1, v2, opt);
            }
            else
            {
                // replace
                v2.WriteTo(writer);
            }

            dict2.Remove(p1.Name); // чтобы потом не дублировать
        }

        // 2. оставшиеся свойства из obj2 (новые)
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

    private static void HandleObject(Utf8JsonWriter writer, JsonElement v1, JsonElement v2, JsonMergeOptions opt, StringComparer comparer)
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
                WriteMergedObject(writer, v1, v2, opt, comparer);
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
        }
    }
}
#endif