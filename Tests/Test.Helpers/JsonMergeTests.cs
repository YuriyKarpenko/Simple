#if NET5_0_OR_GREATER
using System.Text.Json;

namespace Test.Helpers;

public class JsonMergeTests
{
    [Fact]
    public void MergeFast_DeepMergesObjects()
    {
        var options = new JsonSerializerOptions();
        var master = JsonConvertMs.ParseEl("{\"a\":1,\"o\":{\"x\":1}}", options.ToDoc());
        var addon = JsonConvertMs.ParseEl("{\"o\":{\"y\":2}}", options.ToDoc());

        var merged = JsonMerge.MergeFast(ref master, addon, options, JsonMergeOptions.Default);

        Assert.Equal(1, merged.GetProperty("a").GetInt32());
        Assert.Equal(1, merged.GetProperty("o").GetProperty("x").GetInt32());
        Assert.Equal(2, merged.GetProperty("o").GetProperty("y").GetInt32());
    }

    [Fact]
    public void MergeFast_ArrayReplace_IsDefault()
    {
        var options = new JsonSerializerOptions();
        var master = JsonConvertMs.ParseEl("{\"a\":[1,2]}", options.ToDoc());
        var addon = JsonConvertMs.ParseEl("{\"a\":[3]}", options.ToDoc());

        var merged = JsonMerge.MergeFast(ref master, addon, options, JsonMergeOptions.Default);

        Assert.Equal("[3]", merged.GetProperty("a").GetRawText());
    }

    [Fact]
    public void MergeFast_ArrayMerge_AppendsItems()
    {
        var options = new JsonSerializerOptions();
        var mergeOptions = JsonMergeOptions.Default;
        mergeOptions.ArrayBehavior = MergeBehavior.Merge;
        var master = JsonConvertMs.ParseEl("{\"a\":[1,2]}", options.ToDoc());
        var addon = JsonConvertMs.ParseEl("{\"a\":[3,4]}", options.ToDoc());

        var merged = JsonMerge.MergeFast(ref master, addon, options, mergeOptions);

        Assert.Equal("[1,2,3,4]", merged.GetProperty("a").GetRawText());
    }

    [Fact]
    public void MergeFast_IgnoreObject_KeepsOriginal()
    {
        var options = new JsonSerializerOptions();
        var mergeOptions = JsonMergeOptions.Default;
        mergeOptions.ObjectBehavior = MergeBehavior.Ignore;
        var master = JsonConvertMs.ParseEl("{\"o\":{\"x\":1}}", options.ToDoc());
        var addon = JsonConvertMs.ParseEl("{\"o\":{\"y\":2}}", options.ToDoc());

        var merged = JsonMerge.MergeFast(ref master, addon, options, mergeOptions);

        Assert.Equal("{\"x\":1}", merged.GetProperty("o").GetRawText());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MergeFast_NullValue_IsSkippedByDefault(bool isKeepNull)
    {
        var options = new JsonSerializerOptions();
        var master = JsonConvertMs.ParseEl("{\"a\":1}", options.ToDoc());
        var addon = JsonConvertMs.ParseEl("{\"a\":null,\"b\":null}", options.ToDoc());

        JsonMerge.MergeOptions.KeepNullValue = isKeepNull;
        var merged = JsonMerge.MergeFast(ref master, addon, options, null);

        Assert.Equal(isKeepNull, merged.TryGetProperty("a", out var aEl));
        Assert.Equal(isKeepNull, merged.TryGetProperty("b", out _));
        Assert.Equal(isKeepNull ? JsonValueKind.Null : JsonValueKind.Undefined, aEl.ValueKind);
    }

    [Fact]
    public void MergeFast_KeepNullValue_PreservesNull()
    {
        var options = new JsonSerializerOptions();
        var mergeOptions = JsonMergeOptions.Default;
        mergeOptions.KeepNullValue = true;
        var master = JsonConvertMs.ParseEl("{\"a\":1}", options.ToDoc());
        var addon = JsonConvertMs.ParseEl("{\"a\":null,\"b\":null}", options.ToDoc());

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
        var addon = JsonConvertMs.ParseEl("{\"name\":2}", options.ToDoc());

        var merged = JsonMerge.MergeFast(ref master, addon, options, JsonMergeOptions.Default);

        Assert.Equal(2, merged.GetProperty("Name").GetInt32());
    }
}
#endif