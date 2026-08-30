using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Test.Helpers;

public class ExtensionsCollectionTests
{
    [Fact]
    public void GetOrDefault_ReturnsValueOfRequestedType()
    {
        IDictionary<string, object> d = new Dictionary<string, object>
        {
            { "int", 42 },
            { "str", "x" },
        };

        Assert.Equal(42, d.GetOrDefault<int>("int"));
        Assert.Null(d.GetOrDefault<int?>("str"));
        Assert.Null(d.GetOrDefault<int?>("missing"));
    }

    [Fact]
    public void AsString_JoinsWithSeparator()
    {
        Assert.Equal("a, b, c", new[] { "a", "b", "c" }.AsString());
        Assert.Equal("1|2|3", new[] { 1, 2, 3 }.AsString("|"));
    }

    [Fact]
    public void AsString_EmptyEnumerable_ReturnsEmpty()
    {
        Assert.Equal("", Array.Empty<int>().AsString());
    }

    [Fact]
    public void ToNameValueCollection_FlattenGroups()
    {
        IDictionary<string, IEnumerable<string>> source = new Dictionary<string, IEnumerable<string>>
        {
            { "a", new[] { "1", "2" } },
            { "b", new[] { "3" } },
        };

        var result = source.ToNameValueCollection();

        Assert.NotNull(result);
        Assert.Equal("1,2", result["a"]);
        Assert.Equal("3", result["b"]);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ToNameValueCollection_SkipsNullValues()
    {
        IDictionary<string, IEnumerable<string>> source = new Dictionary<string, IEnumerable<string>>
        {
            { "a", new[] { "1", "2" } },
            { "b", null! },
        };

        var result = source.ToNameValueCollection();

        Assert.Single(result.AllKeys);
        Assert.Equal("1,2", result["a"]);
    }

    [Fact]
    public void Merge_OverEnumerable_CombinesDictionaries()
    {
        IEnumerable<IDictionary<string, int>> list = new[]
        {
            new Dictionary<string, int> { { "a", 1 } },
            new Dictionary<string, int> { { "b", 2 } },
            null!,
        };

        var result = list.Merge();

        Assert.Equal(1, result["a"]);
        Assert.Equal(2, result["b"]);
    }

    [Fact]
    public void Merge_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => Simple.Helpers.ExtensionsCollection.Merge<int>(null!));
        Assert.Throws<ArgumentNullException>(() => new Dictionary<string, int>().Merge(null!));
    }

    [Fact]
    public void Merge_OverEnumerable_FirstOneWins_ForDuplicateKeys()
    {
        IEnumerable<IDictionary<string, int>> list = new[]
        {
            new Dictionary<string, int> { { "a", 1 } },
            new Dictionary<string, int> { { "a", 5 } },
        };

        var result = list.Merge();

        Assert.Equal(1, result["a"]);
    }

    [Fact]
    public void DistinctBy_FiltersDuplicates()
    {
        var items = new[] { "apple", "Ape", "Banana", "cherry" };

        var result = items.DistinctBy(s => s.Length).ToArray();

        Assert.Equal(new[] { "apple", "Ape", "Banana" }, result);
    }

    [Fact]
    public void Merge_WithIsOverrideFalse_KeepsExisting()
    {
        var dst = new Dictionary<string, int> { { "a", 1 } };
        var src = new Dictionary<string, int> { { "a", 9 }, { "b", 2 } };

        var result = dst.Merge(src, false);

        Assert.Equal(1, result["a"]);
        Assert.Equal(2, result["b"]);
    }

    [Fact]
    public void TryGet_TypeMismatch_ReturnsFalse()
    {
        IDictionary<string, object> d = new Dictionary<string, object> { { "k", "str" } };

        var ok = d.TryGet("k", out int result);

        Assert.False(ok);
        Assert.Equal(0, result);
    }

    [Fact]
    public void TryGet_ReadOnly_MissingKey_ReturnsFalse()
    {
        IReadOnlyDictionary<string, object> d = new Dictionary<string, object> { { "k", 1 } };

        var ok = d.TryGet("missing", out int result);

        Assert.False(ok);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Merge_WithIsOverrideKeys(bool isOverrideKeys)
    {
        var d1 = new DicString<int>
        {
            { "1", 1 },
            { "2", 2 },
        };
        IDictionary<string, int> d2 = new DicString<int>
        {
            { "2", 4 },
            { "3", 3 },
        };
        var expected = new DicString<int>
        {
            { "1", 1 },
            { "2", isOverrideKeys ? 4 : 2 },
            { "3", 3 },
        };

        var actual = d1.Merge(d2, isOverrideKeys);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(44)]
    public void TryGet_ReturnsValue(int? expected)
    {
        IDictionary<string, object?> d1 = new DicString<object?>
        {
            { "1", 1 },
            { "2", expected },
        };

        var hasValue = d1.TryGet("2", out int? actual);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(44)]
    public void TryGet_ReadOnly_ReturnsValue(int? expected)
    {
        IReadOnlyDictionary<string, object?> d1 = new DicString<object?>
        {
            { "1", 1 },
            { "2", expected },
        };

        var hasValue = d1.TryGet("2", out int? actual);

        Assert.Equal(expected, actual);
    }
}