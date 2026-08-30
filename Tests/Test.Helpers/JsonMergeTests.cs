#if NET5_0_OR_GREATER
using System;
using System.Text.Json;

namespace Test.Helpers;

public class JsonMergeTests
{
    [Fact]
    public void MergeFast_DeepMergesObjects()
    {
        var options = new JsonSerializerOptions();
        var master  = JsonConvertMs.ParseEl("{\"a\":1,\"o\":{\"x\":1}}", options.ToDoc());
        var addon   = JsonConvertMs.ParseEl("{\"o\":{\"y\":2}}", options.ToDoc());

        var merged = JsonMerge.MergeFast(ref master, addon, options, JsonMergeOptions.Default);

        Assert.Equal(1, merged.GetProperty("a").GetInt32());
        Assert.Equal(1, merged.GetProperty("o").GetProperty("x").GetInt32());
        Assert.Equal(2, merged.GetProperty("o").GetProperty("y").GetInt32());
    }

    [Fact]
    public void MergeFast_ArrayReplace_IsDefault()
    {
        var options = new JsonSerializerOptions();
        var master  = JsonConvertMs.ParseEl("{\"a\":[1,2]}", options.ToDoc());
        var addon   = JsonConvertMs.ParseEl("{\"a\":[3]}", options.ToDoc());

        var merged = JsonMerge.MergeFast(ref master, addon, options, JsonMergeOptions.Default);

        Assert.Equal("[3]", merged.GetProperty("a").GetRawText());
    }

    [Fact]
    public void MergeFast_ArrayMerge_AppendsItems()
    {
        var options      = new JsonSerializerOptions();
        var mergeOptions = JsonMergeOptions.Default;
        mergeOptions.ArrayBehavior = MergeBehavior.Merge;
        var master = JsonConvertMs.ParseEl("{\"a\":[1,2]}", options.ToDoc());
        var addon  = JsonConvertMs.ParseEl("{\"a\":[3,4]}", options.ToDoc());

        var merged = JsonMerge.MergeFast(ref master, addon, options, mergeOptions);

        Assert.Equal("[1,2,3,4]", merged.GetProperty("a").GetRawText());
    }

    [Fact]
    public void MergeFast_IgnoreObject_KeepsOriginal()
    {
        var options      = new JsonSerializerOptions();
        var mergeOptions = JsonMergeOptions.Default;
        mergeOptions.ObjectBehavior = MergeBehavior.Ignore;
        var master = JsonConvertMs.ParseEl("{\"o\":{\"x\":1}}", options.ToDoc());
        var addon  = JsonConvertMs.ParseEl("{\"o\":{\"y\":2}}", options.ToDoc());

        var merged = JsonMerge.MergeFast(ref master, addon, options, mergeOptions);

        Assert.Equal("{\"x\":1}", merged.GetProperty("o").GetRawText());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MergeFast_NullValue_IsSkippedByDefault(bool isKeepNull)
    {
        var options = new JsonSerializerOptions();
        var master  = JsonConvertMs.ParseEl("{\"a\":1}", options.ToDoc());
        var addon   = JsonConvertMs.ParseEl("{\"a\":null,\"b\":null}", options.ToDoc());

        var mergeOptions = JsonMergeOptions.Default;
        mergeOptions.KeepNullValue = isKeepNull;
        var merged = JsonMerge.MergeFast(ref master, addon, options, mergeOptions);

        Assert.Equal(isKeepNull, merged.TryGetProperty("a", out var aEl));
        Assert.Equal(isKeepNull, merged.TryGetProperty("b", out _));
        Assert.Equal(isKeepNull ? JsonValueKind.Null : JsonValueKind.Undefined, aEl.ValueKind);
    }

    [Fact]
    public void MergeFast_KeepNullValue_PreservesNull()
    {
        var options      = new JsonSerializerOptions();
        var mergeOptions = JsonMergeOptions.Default;
        mergeOptions.KeepNullValue = true;
        var master = JsonConvertMs.ParseEl("{\"a\":1}", options.ToDoc());
        var addon  = JsonConvertMs.ParseEl("{\"a\":null,\"b\":null}", options.ToDoc());

        var merged = JsonMerge.MergeFast(ref master, addon, options, mergeOptions);

        Assert.Equal(JsonValueKind.Null, merged.GetProperty("a").ValueKind);
        Assert.Equal(JsonValueKind.Null, merged.GetProperty("b").ValueKind);
    }

    [Fact]
    public void MergeFast_CaseInsensitive_UsesOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        var master = JsonConvertMs.ParseEl("{\"Name\":1}", options.ToDoc());
        var addon  = JsonConvertMs.ParseEl("{\"name\":2}", options.ToDoc());

        var merged = JsonMerge.MergeFast(ref master, addon, options, JsonMergeOptions.Default);

        Assert.Equal(2, merged.GetProperty("Name").GetInt32());
    }

    private static string DeepJson(int depth, string leaf)
    {
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < depth; i++)
        {
            sb.Append("{\"a\":");
        }

        sb.Append(leaf);
        sb.Append('}', depth);
        return sb.ToString();
    }

    [Fact]
    public void MergeFast_DepthLimit_Throws()
    {
        var options = new JsonSerializerOptions { MaxDepth = 200 };
        var master  = JsonConvertMs.ParseEl(DeepJson(70, "1"), options.ToDoc());
        var addon   = JsonConvertMs.ParseEl(DeepJson(70, "2"), options.ToDoc());

        var ex = Assert.Throws<JsonException>(() => JsonMerge.MergeFast(ref master, addon, options));

        Assert.Contains("depth", ex.Message);
    }

    [Fact]
    public void Default_Equals_DefaultStruct()
    {
        var d = default(JsonMergeOptions);

        Assert.Equal(MergeBehavior.Replace, d.ArrayBehavior);
        Assert.Equal(MergeBehavior.Merge, d.ObjectBehavior);
        Assert.False(d.KeepNullValue);
        Assert.Equal(JsonMergeOptions.Default.ArrayBehavior, d.ArrayBehavior);
        Assert.Equal(JsonMergeOptions.Default.ObjectBehavior, d.ObjectBehavior);
        Assert.Equal(JsonMergeOptions.Default.KeepNullValue, d.KeepNullValue);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void MergeFast_BothEmpty_ReturnsEmptyObject(string? json)
    {
        var merged = JsonMerge.MergeFast(json, json);

        Assert.Equal(JsonValueKind.Object, merged.ValueKind);
        Assert.Empty(merged.EnumerateObject());
    }

    [Fact]
    public void MergeFast_MasterEmpty_ReturnsAddonObject()
    {
        var merged = JsonMerge.MergeFast(null, "{\"b\":2}");

        Assert.Equal(JsonValueKind.Object, merged.ValueKind);
        Assert.Equal(2, merged.GetProperty("b").GetInt32());
    }

    [Fact]
    public void MergeFast_AddonEmpty_ReturnsMaster()
    {
        var merged = JsonMerge.MergeFast("{\"a\":1}", null);

        Assert.Equal(1, merged.GetProperty("a").GetInt32());
    }

    [Theory]
    [InlineData("[1]", JsonValueKind.Array)]
    [InlineData("null", JsonValueKind.Null)]
    [InlineData("123", JsonValueKind.Number)]
    [InlineData("\"s\"", JsonValueKind.String)]
    public void MergeFast_MasterEmpty_AddonPassthrough(string addon, JsonValueKind kind)
    {
        var merged = JsonMerge.MergeFast(null, addon);

        Assert.Equal(kind, merged.ValueKind);
    }

    [Fact]
    public void MergeFast_AddonEmpty_MasterPassthrough()
    {
        var merged = JsonMerge.MergeFast("[1]", null);

        Assert.Equal(JsonValueKind.Array, merged.ValueKind);
    }

    [Fact]
    public void MergeFast_BothNonEmpty_MasterNotObject_Throws()
    {
        var ex = Assert.Throws<JsonException>(() => JsonMerge.MergeFast("[1]", "{\"a\":1}"));

        Assert.Contains("master must be an object", ex.Message);
    }

    [Fact]
    public void MergeFast_WhitespaceMaster_Throws()
    {
        Assert.ThrowsAny<JsonException>(() => JsonMerge.MergeFast("  ", null));
    }

    [Fact]
    public void MergeFast_UnknownArrayBehavior_Throws()
    {
        var options = new JsonSerializerOptions();
        var master  = JsonConvertMs.ParseEl("{\"a\":[1]}", options.ToDoc());
        var addon   = JsonConvertMs.ParseEl("{\"a\":[2]}", options.ToDoc());
        var opt     = new JsonMergeOptions { ArrayBehavior = (MergeBehavior)999 };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => JsonMerge.MergeFast(ref master, addon, options, opt));

        Assert.Contains("unknown ArrayBehavior", ex.Message);
    }

    [Fact]
    public void MergeFast_UnknownObjectBehavior_Throws()
    {
        var options = new JsonSerializerOptions();
        var master  = JsonConvertMs.ParseEl("{\"o\":{\"x\":1}}", options.ToDoc());
        var addon   = JsonConvertMs.ParseEl("{\"o\":{\"y\":2}}", options.ToDoc());
        var opt     = new JsonMergeOptions { ObjectBehavior = (MergeBehavior)999 };

        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => JsonMerge.MergeFast(ref master, addon, options, opt));

        Assert.Contains("unknown ObjectBehavior", ex.Message);
    }

    [Fact]
    public void MergeFast_DuplicateAddonKeys_LastWins()
    {
        var merged = JsonMerge.MergeFast("{\"a\":1}", "{\"a\":2,\"a\":3}");

        Assert.Equal(3, merged.GetProperty("a").GetInt32());
    }

    [Fact]
    public void MergeFast_AddonNotObject_Throws()
    {
        var ex = Assert.Throws<JsonException>(() => JsonMerge.MergeFast("{\"a\":1}", "[1]"));

        Assert.Contains("addon must be an object", ex.Message);
    }

    [Fact]
    public void MergeFast_ObjectReplace_OverridesMasterObject()
    {
        var options      = new JsonSerializerOptions();
        var mergeOptions = JsonMergeOptions.Default;
        mergeOptions.ObjectBehavior = MergeBehavior.Replace;
        var master = JsonConvertMs.ParseEl("{\"o\":{\"x\":1}}", options.ToDoc());
        var addon  = JsonConvertMs.ParseEl("{\"o\":{\"y\":2}}", options.ToDoc());

        var merged = JsonMerge.MergeFast(ref master, addon, options, mergeOptions);

        Assert.Equal("{\"y\":2}", merged.GetProperty("o").GetRawText());
    }

    [Fact]
    public void MergeFast_ArrayIgnore_KeepsMasterArray()
    {
        var options      = new JsonSerializerOptions();
        var mergeOptions = JsonMergeOptions.Default;
        mergeOptions.ArrayBehavior = MergeBehavior.Ignore;
        var master = JsonConvertMs.ParseEl("{\"a\":[1,2]}", options.ToDoc());
        var addon  = JsonConvertMs.ParseEl("{\"a\":[3]}", options.ToDoc());

        var merged = JsonMerge.MergeFast(ref master, addon, options, mergeOptions);

        Assert.Equal("[1,2]", merged.GetProperty("a").GetRawText());
    }
}
#endif