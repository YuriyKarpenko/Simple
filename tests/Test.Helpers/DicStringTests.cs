using System;
using System.Collections.Generic;

namespace Test.Helpers;

public class DicStringTests
{
    private enum MyEnum
    {
        Alpha,
        Beta,
    }

    [Fact]
    public void ToEnumKey_ConvertsStringsToEnum()
    {
        var d = new DicString<int>
        {
            { "Alpha", 1 },
            { "beta", 2 },
        };

        var result = d.ToEnumKey<MyEnum>();

        Assert.Equal(1, result[MyEnum.Alpha]);
        Assert.Equal(2, result[MyEnum.Beta]);
    }

    [Fact]
    public void ToEnumKey_CaseSensitive_FiltersUnknown()
    {
        var d = new DicString<int>
        {
            { "Alpha", 1 },
            { "beta", 2 },
        };

        var result = d.ToEnumKey<MyEnum>(false);

        Assert.Single(result);
        Assert.Equal(1, result[MyEnum.Alpha]);
    }

    [Fact]
    public void ToEnumKey_SkipsNonEnumKeys()
    {
        var d = new DicString<int>
        {
            { "Alpha", 1 },
            { "not-an-enum", 2 },
            { "42", 3 }, //  numeric string: parses, but not a defined member
        };

        var result = d.ToEnumKey<MyEnum>();

        Assert.Single(result);
    }

    [Fact]
    public void DefaultComparer_IsCaseInsensitive()
    {
        var d = new DicString<int>();
        d["key"] = 1;

        Assert.Equal(1, d["KEY"]);
        Assert.True(d.ContainsKey("KeY"));
    }

    [Fact]
    public void Ctor_FromIdictionary_Works()
    {
        IDictionary<string, int> source = new Dictionary<string, int> { { "a", 1 } };

        var d = new DicString<int>(source);

        Assert.Equal(1, d["a"]);
    }

    [Fact]
    public void Ctor_WithCapacity_Works()
    {
        var d = new DicString<int>(4);

        d["a"] = 1;

        Assert.Equal(1, d["A"]);
    }

    [Fact]
    public void Ctor_WithCustomComparer_UseIt()
    {
        var d = new DicString<int>(StringComparer.Ordinal);
        d["a"] = 1;

        Assert.False(d.ContainsKey("A"));
    }

    [Fact]
    public void Ctor_FromEnumerableOfKeyValuePairs_Works()
    {
        var source = new List<KeyValuePair<string, int>>
        {
            new("a", 1),
            new("b", 2),
        };

        var d = new DicString<int>(source);

        Assert.Equal(1, d["A"]);
        Assert.Equal(2, d["b"]);
    }

    [Fact]
    public void Ctor_FromEnumerable_WithCustomComparer_UsesIt()
    {
        var source = new List<KeyValuePair<string, int>> { new("a", 1) };

        var d = new DicString<int>(source, StringComparer.Ordinal);

        Assert.False(d.ContainsKey("A"));
    }
}