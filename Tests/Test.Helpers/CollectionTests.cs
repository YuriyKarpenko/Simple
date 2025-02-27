using System.Collections.Generic;

namespace Test.Helpers;
public class CollectionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Merge(bool isOverrideKeys)
    {
        //  arrange
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

        //  test
        var actual = d1.Merge(d2, isOverrideKeys);

        //  assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MergeReadOnly(bool isOverrideKeys)
    {
        //  arrange
        var d1 = new DicString<int>
        {
            { "1", 1 },
            { "2", 2 },
        };
        IReadOnlyDictionary<string, int> d2 = new DicString<int>
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

        //  test
        var actual = d1.Merge(d2, isOverrideKeys);

        //  assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(44)]
    public void TryGet(int? expected)
    {
        //  arrange
        IDictionary<string, object?> d1 = new DicString<object?>
        {
            { "1", 1 },
            { "2", expected },
        };

        //  test
        var hasValue = d1.TryGet("2", out int? actual);

        //  assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(44)]
    public void TryGetReadOnly(int? expected)
    {
        //  arrange
        IReadOnlyDictionary<string, object?> d1 = new DicString<object?>
        {
            { "1", 1 },
            { "2", expected },
        };

        //  test
        var hasValue = d1.TryGet("2", out int? actual);

        //  assert
        Assert.Equal(expected, actual);
    }
}
