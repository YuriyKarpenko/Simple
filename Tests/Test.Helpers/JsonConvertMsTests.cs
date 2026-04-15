using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Test.Helpers;

public class JsonConvertMsTests
{
    private enum MyEnum
    {
        Alpha,
        Beta,
    }

    private sealed class Sample
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public MyEnum Mode { get; set; }
    }

    private static JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Default;

    [Fact]
    public void SerializeObject_And_DeserializeObject_RoundTrip()
    {
        var jsonOptions = new JsonSerializerOptions(JsonConvertMs.JsonOptions);
        jsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        var sample = new Sample { Id = 7, Name = "x", Mode = MyEnum.Beta };

        var json = JsonConvertMs.SerializeObject(sample, jsonOptions);
        var result = JsonConvertMs.DeserializeObject<Sample>(json, jsonOptions);

        Assert.NotNull(json);
        Assert.NotNull(result);
        Assert.Contains("Beta", json, StringComparison.Ordinal);
        Assert.Equal(sample.Id, result.Id);
        Assert.Equal(sample.Name, result.Name);
        Assert.Equal(sample.Mode, result.Mode);
    }

    [Fact]
    public void DeserializeObject_Empty_ReturnsDefault()
    {
        var result = JsonConvertMs.DeserializeObject<int?>(string.Empty, _jsonOptions);

        Assert.Null(result);
    }

    [Fact]
    public void FromObject_And_CreateObject_Work()
    {
        var obj = JsonConvertMs.FromObject(new Sample { Id = 1, Name = "n", Mode = MyEnum.Alpha }, _jsonOptions);
        var element = JsonConvertMs.ParseEl(obj.ToJsonString(), _jsonOptions.ToDoc());
        var recreated = JsonConvertMs.CreateObject(element, _jsonOptions.ToNode());

        Assert.NotNull(recreated);
        Assert.Equal(1, recreated["Id"]!.GetValue<int>());
    }

    [Fact]
    public void ParseEl_ReturnsClonedElement_ThatSurvivesMethodScope()
    {
        var element = JsonConvertMs.ParseEl("{\"a\":1}", _jsonOptions.ToDoc());

        Assert.Equal(JsonValueKind.Object, element.ValueKind);
        Assert.Equal(1, element.GetProperty("a").GetInt32());
    }

    [Fact]
    public void ParseObj_And_Clone_Work()
    {
        var obj = JsonConvertMs.ParseObj("{\"a\":1}", _jsonOptions);
        var clone = obj.DeepClone();
        clone["a"] = 2;

        Assert.Equal(1, obj["a"]!.GetValue<int>());
        Assert.Equal(2, clone["a"]!.GetValue<int>());
    }

    [Fact]
    public void Merge_JsonObjects_Works()
    {
        var master = JsonConvertMs.ParseObj("{\"a\":1,\"o\":{\"x\":1}}", _jsonOptions);
        var addon = JsonConvertMs.ParseObj("{\"b\":2,\"o\":{\"y\":2}}", _jsonOptions);

        JsonConvertMs.Merge(master, addon);

        Assert.Equal(1, master["a"]!.GetValue<int>());
        Assert.Equal(2, master["b"]!.GetValue<int>());
        Assert.Equal(1, master["o"]!["x"]!.GetValue<int>());
        Assert.Equal(2, master["o"]!["y"]!.GetValue<int>());
    }

    [Fact]
    public void Options_Extensions_Work()
    {
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas         = true,
            PropertyNameCaseInsensitive = true,
            WriteIndented               = true,
            MaxDepth                    = 8,
        };

        var doc = options.ToDoc();
        var node = options.ToNode();
        var read = options.ToRead();
        var write = options.ToWrite();

        Assert.True(doc.AllowTrailingCommas);
        Assert.True(node.PropertyNameCaseInsensitive);
        Assert.True(read.AllowTrailingCommas);
        Assert.True(write.Indented);
    }

#if NET5_0_OR_GREATER
    [Fact]
    public void FindDiff_And_PropertyNames_Work()
    {
        var current = JsonNode.Parse("{\"a\":1,\"b\":[1,2],\"c\":3}")!.AsObject();
        var old = JsonNode.Parse("{\"a\":1,\"b\":[2],\"d\":4}")!.AsObject();

        var diff = current.FindDiff(old);
        var names = current.PropertyNames();

        Assert.Contains("b", diff.Select(kv => kv.Key));
        Assert.Contains("c", diff.Select(kv => kv.Key));
        Assert.Contains("d", diff.Select(kv => kv.Key));
        Assert.Contains("a", names);
        Assert.Contains("b", names);
    }
#endif
}