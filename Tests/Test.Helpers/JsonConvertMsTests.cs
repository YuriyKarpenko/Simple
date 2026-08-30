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

        var json   = JsonConvertMs.SerializeObject(sample, jsonOptions);
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
    public void DeserializeObject_Whitespace_Throws()
    {
        //  whitespace is not "empty": passes through to the parser, which throws (consistent with span overloads and ParseEl/ParseObj)
        Assert.ThrowsAny<JsonException>(() => JsonConvertMs.DeserializeObject<int?>("  ", _jsonOptions));
    }

    [Fact]
    public void ParseDoc_Null_ThrowsArgumentNullException()
    {
        Assert.ThrowsAny<ArgumentNullException>(() => JsonConvertMs.ParseDoc((string?)null, default(JsonDocumentOptions)));
    }

    [Fact]
    public void ParseDoc_Empty_ThrowsJsonException()
    {
        Assert.ThrowsAny<JsonException>(() => JsonConvertMs.ParseDoc(string.Empty, default(JsonDocumentOptions)));
    }

    [Fact]
    public void FromObject_And_CreateObject_Work()
    {
        var obj       = JsonConvertMs.FromObject(new Sample { Id = 1, Name = "n", Mode = MyEnum.Alpha }, _jsonOptions);
        var element   = JsonConvertMs.ParseEl(obj.ToJsonString(), _jsonOptions.ToDoc());
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
        var obj   = JsonConvertMs.ParseObj("{\"a\":1}", _jsonOptions);
        var clone = obj.DeepClone();
        clone["a"] = 2;

        Assert.Equal(1, obj["a"]!.GetValue<int>());
        Assert.Equal(2, clone["a"]!.GetValue<int>());
    }

    [Theory]
    [InlineData("[1,2]", "Array")]
    [InlineData("123", "Number")]
    [InlineData("\"s\"", "String")]
    [InlineData("true", "True")]
    [InlineData("null", "Null")]
    public void ParseObj_NonObject_Throws(string json, string kind)
    {
        var ex = Assert.Throws<JsonException>(() => JsonConvertMs.ParseObj(json, _jsonOptions));

        Assert.Contains("expected a JSON object", ex.Message, StringComparison.Ordinal);
        Assert.Contains(kind, ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ParseObj_NullOrEmpty_ReturnsEmptyObject(string? json)
    {
        var obj = JsonConvertMs.ParseObj(json, _jsonOptions);

        Assert.Empty(obj);
    }

    [Fact]
    public void FromObject_NonObject_Throws()
    {
        Assert.Throws<JsonException>(() => JsonConvertMs.FromObject(new[] { 1, 2 }, _jsonOptions));
        Assert.Throws<JsonException>(() => JsonConvertMs.FromObject("s", _jsonOptions));
        Assert.Throws<JsonException>(() => JsonConvertMs.FromObject(42, _jsonOptions));
    }

    [Fact]
    public void FromObject_Null_ReturnsEmptyObject()
    {
        Assert.Empty(JsonConvertMs.FromObject((object?)null, _jsonOptions));
    }

    [Fact]
    public void CreateObject_NonObject_Throws()
    {
        var arr = JsonConvertMs.ParseEl("[1]", _jsonOptions.ToDoc());

        var arrEx = Assert.Throws<JsonException>(() => JsonConvertMs.CreateObject(arr, _jsonOptions.ToNode()));
        var undEx = Assert.Throws<JsonException>(() => JsonConvertMs.CreateObject(default, _jsonOptions.ToNode()));

        Assert.Contains("Array", arrEx.Message, StringComparison.Ordinal);
        Assert.Contains("Undefined", undEx.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Merge_JsonObjects_Works()
    {
        var master = JsonConvertMs.ParseObj("{\"a\":1,\"o\":{\"x\":1}}", _jsonOptions);
        var addon  = JsonConvertMs.ParseObj("{\"b\":2,\"o\":{\"y\":2}}", _jsonOptions);

#pragma warning disable CS0618 // obsolete Merge(JsonObject, JsonObject)
        JsonConvertMs.Merge(master, addon);
#pragma warning restore CS0618

        Assert.Equal(1, master["a"]!.GetValue<int>());
        Assert.Equal(2, master["b"]!.GetValue<int>());
        Assert.Equal(1, master["o"]!["x"]!.GetValue<int>());
        Assert.Equal(2, master["o"]!["y"]!.GetValue<int>());
    }

    private static JsonObject DeepObject(int depth, int leaf)
    {
        var root = new JsonObject();
        var cur  = root;
        for (var i = 0; i < depth; i++)
        {
            var next = new JsonObject();
            cur["a"] = next;
            cur      = next;
        }

        cur["v"] = leaf;
        return root;
    }

    [Fact]
    public void Merge_DepthLimit_Throws()
    {
#pragma warning disable CS0618 // obsolete Merge(JsonObject, JsonObject)
        var ex = Assert.Throws<JsonException>(() => JsonConvertMs.Merge(DeepObject(70, 1), DeepObject(70, 2)));
#pragma warning restore CS0618

        Assert.Contains("depth", ex.Message);
    }

    [Fact]
    public void Merge_SameObject_IsNoOp()
    {
        var obj = JsonConvertMs.ParseObj("{\"a\":1,\"o\":{\"x\":1}}", _jsonOptions);

#pragma warning disable CS0618 // obsolete Merge(JsonObject, JsonObject)
        JsonConvertMs.Merge(obj, obj);
#pragma warning restore CS0618

        Assert.Equal(1, obj["a"]!.GetValue<int>());
        Assert.Equal(1, obj["o"]!["x"]!.GetValue<int>());
    }

    [Fact]
    public void FindDiff_DepthLimit_Throws()
    {
        var ex = Assert.Throws<JsonException>(() => DeepObject(70, 1).FindDiff(DeepObject(70, 2)));

        Assert.Contains("depth", ex.Message);
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

        var doc   = options.ToDoc();
        var node  = options.ToNode();
        var read  = options.ToRead();
        var write = options.ToWrite();

        Assert.True(doc.AllowTrailingCommas);
        Assert.True(node.PropertyNameCaseInsensitive);
        Assert.True(read.AllowTrailingCommas);
        Assert.True(write.Indented);
    }

    [Fact]
    public void JsonOptions_Frozen_MutationThrowsAtCallSite()
    {
        //  заморожены сразу при инициализации типа — ошибка видна в месте мутации
        Assert.ThrowsAny<InvalidOperationException>(() =>
            JsonConvertMs.JsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
    }

    [Fact]
    public void ToWrite_ValidationEnabled()
    {
        //  SkipValidation=false: writer валидирует выход, битый JSON невозможен
        Assert.False(_jsonOptions.ToWrite().SkipValidation);
    }

    [Fact]
    public void CreateOptions_ReturnsMutableCopy()
    {
        var options = JsonConvertMs.CreateOptions();

        options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

        Assert.Contains(options.Converters, c => c is System.Text.Json.Serialization.JsonStringEnumConverter);
        //  исходник заморожен и не затронут попыткой мутации копии
        Assert.ThrowsAny<InvalidOperationException>(() => JsonConvertMs.JsonOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
    }

#if NET5_0_OR_GREATER
    [Fact]
    public void FindDiff_And_PropertyNames_Work()
    {
        var current = JsonNode.Parse("{\"a\":1,\"b\":[1,2],\"c\":3}")!.AsObject();
        var old     = JsonNode.Parse("{\"a\":1,\"b\":[2],\"d\":4}")!.AsObject();

        var diff  = current.FindDiff(old);
        var names = current.PropertyNames();

        Assert.Contains("b", diff.Select(kv => kv.Key));
        Assert.Contains("c", diff.Select(kv => kv.Key));
        Assert.Contains("d", diff.Select(kv => kv.Key));
        Assert.Contains("a", names);
        Assert.Contains("b", names);
    }

    [Fact]
    public void FindDiff_ArraysWithSameObjectsDifferentOrder_ReportedAsDiff()
    {
        var current = JsonNode.Parse("{\"a\":[{\"x\":1,\"y\":2},1]}")!.AsObject();
        var old     = JsonNode.Parse("{\"a\":[{\"y\":2,\"x\":1},2]}")!.AsObject();

        var diff  = current.FindDiff(old);
        var minus = diff["a"]!["-"]!.AsArray();

        //  documented caveat of ToJsonString-keyed array diff: property order matters inside array items
        Assert.Equal(2, minus.Count);
    }

    [Fact]
    public void FindDiff_CaseInsensitiveKeys_NotReportedAsDiff()
    {
        var nOpt    = new JsonNodeOptions { PropertyNameCaseInsensitive = true };
        var current = JsonNode.Parse("{\"Key\":1,\"z\":2}", nOpt)!.AsObject();
        var old     = JsonNode.Parse("{\"key\":1,\"z\":2}", nOpt)!.AsObject();

        var diff = current.FindDiff(old, nOpt);

        //  "Key"/"key" — один ключ при включённой опции: не добавлен, не удалён
        Assert.Empty(diff);
    }

    [Fact]
    public void FindDiff_ModifiedScalar_ReportedAsDiff()
    {
        var current = JsonNode.Parse("{\"a\":1}")!.AsObject();
        var old     = JsonNode.Parse("{\"a\":2}")!.AsObject();

        var diff = current.FindDiff(old);
        var a    = diff["a"]!.AsObject();

        Assert.Equal(1, a["+"]!.GetValue<int>());
        Assert.Equal(2, a["-"]!.GetValue<int>());
    }

    [Fact]
    public void FindDiff_TypeChanged_ReportedAsDiff()
    {
        var current = JsonNode.Parse("{\"a\":{\"x\":1}}")!.AsObject();
        var old     = JsonNode.Parse("{\"a\":1}")!.AsObject();

        var diff = current.FindDiff(old);
        var a    = diff["a"]!.AsObject();

        Assert.Equal(JsonValueKind.Object, a["+"]!.GetValueKind());
        Assert.Single(a["+"]!.AsObject());
        Assert.Equal(1, a["-"]!.GetValue<int>());
    }
#endif

    [Fact]
    public void DeserializeObject_Utf8Span_RoundTrips()
    {
        var result = JsonConvertMs.DeserializeObject<int>("123"u8, _jsonOptions);

        Assert.Equal(123, result);
    }

    [Fact]
    public void DeserializeObject_StringObsolete_Null_ReturnsDefault()
    {
#pragma warning disable CS0618 // obsolete DeserializeObject<T>(string?)
        var result = JsonConvertMs.DeserializeObject<Sample?>((string?)null);
#pragma warning restore CS0618

        Assert.Null(result);
    }

    [Fact]
    public void DeserializeObject_Node_RoundTrips()
    {
        var node = JsonNode.Parse("{\"Id\":5}")!;

        var result = JsonConvertMs.DeserializeObject<Sample>(node, _jsonOptions);

        Assert.Equal(5, result!.Id);
    }

    [Fact]
    public void DeserializeObject_NodeObsolete_RoundTrips()
    {
        var node = JsonNode.Parse("{\"Id\":6}")!;

#pragma warning disable CS0618 // obsolete DeserializeObject<T>(JsonNode?)
        var result = JsonConvertMs.DeserializeObject<Sample>(node);
#pragma warning restore CS0618

        Assert.Equal(6, result!.Id);
    }

    [Fact]
    public void SerializeObject_Obsolete_RoundTrips()
    {
#pragma warning disable CS0618 // obsolete SerializeObject<T>(T?)
        var json = JsonConvertMs.SerializeObject(new Sample { Id = 9 });
#pragma warning restore CS0618

        Assert.Contains("\"Id\":9", json, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateObject_FromElementObsolete_RoundTrips()
    {
        var element = JsonConvertMs.ParseEl("{\"a\":1}", _jsonOptions.ToDoc());

#pragma warning disable CS0618 // obsolete CreateObject(JsonElement)
        var obj = JsonConvertMs.CreateObject(element);
#pragma warning restore CS0618

        Assert.Equal(1, obj["a"]!.GetValue<int>());
    }

    [Fact]
    public void FromObject_Obsolete_RoundTrips()
    {
#pragma warning disable CS0618 // obsolete FromObject<T>(T?)
        var obj = JsonConvertMs.FromObject(new Sample { Id = 3 });
#pragma warning restore CS0618

        Assert.Equal(3, obj["Id"]!.GetValue<int>());
    }

    [Fact]
    public void ParseDoc_Utf8Memory_RoundTrips()
    {
        var utf8 = "{}"u8.ToArray();
        var doc  = JsonConvertMs.ParseDoc(utf8, default(JsonDocumentOptions));

        Assert.Equal(JsonValueKind.Object, doc.RootElement.ValueKind);
    }

    [Fact]
    public void ParseDoc_StringConvenience_RoundTrips()
    {
        var doc = JsonConvertMs.ParseDoc("{\"a\":1}");

        Assert.Equal(1, doc.RootElement.GetProperty("a").GetInt32());
    }

    [Fact]
    public void ParseEl_Utf8Memory_RoundTrips()
    {
        var utf8    = "{\"a\":1}"u8.ToArray();
        var element = JsonConvertMs.ParseEl(utf8, _jsonOptions.ToDoc());

        Assert.Equal(1, element.GetProperty("a").GetInt32());
    }

    [Fact]
    public void ParseEl_StringObsolete_Null_ReturnsDefault()
    {
#pragma warning disable CS0618 // obsolete ParseEl(string?)
        var element = JsonConvertMs.ParseEl((string?)null);
#pragma warning restore CS0618

        Assert.Equal(JsonValueKind.Undefined, element.ValueKind);
    }

    [Fact]
    public void ParseObj_Utf8Span_RoundTrips()
    {
        var obj = JsonConvertMs.ParseObj("{\"a\":1}"u8, _jsonOptions);

        Assert.Equal(1, obj["a"]!.GetValue<int>());
    }

    [Fact]
    public void ParseObj_StringObsolete_RoundTrips()
    {
#pragma warning disable CS0618 // obsolete ParseObj(string?)
        var obj = JsonConvertMs.ParseObj("{\"a\":1}");
#pragma warning restore CS0618

        Assert.Equal(1, obj["a"]!.GetValue<int>());
    }

    [Fact]
    public void Merge_MasterWithJsonStringAddon_Merges()
    {
        var master = JsonConvertMs.ParseObj("{\"a\":1}", _jsonOptions);

#pragma warning disable CS0618 // obsolete Merge(JsonObject, string)
        JsonConvertMs.Merge(master, "{\"b\":2}");
#pragma warning restore CS0618

        Assert.Equal(1, master["a"]!.GetValue<int>());
        Assert.Equal(2, master["b"]!.GetValue<int>());
    }
}