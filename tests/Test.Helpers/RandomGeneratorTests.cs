using System;
using System.Linq;

namespace Test.Helpers;

public class RandomGeneratorTests
{
    [Fact]
    public void Fill_NotAllZeroBytes()
    {
        var bb = new byte[10];

        RandomGenerator.Fill(bb);

        Assert.NotEqual(new byte[10], bb);
    }

    [Fact]
    public void Shuffle_ChangesOrder()
    {
        Span<byte> span = stackalloc byte[10];
        RandomGenerator.Fill(span);
        var before = span.ToArray();

        RandomGenerator.Shuffle(span);

        Assert.NotEqual(before, span.ToArray());
    }

    [Fact]
    public void GetInt_RespectsRange()
    {
        for (var i = 0; i < 100; i++)
        {
            var v = RandomGenerator.GetInt(5, 10);
            Assert.InRange(v, 5, 9);
        }
    }

    [Fact]
    public void GetInt_ToExclusive_RespectsRange()
    {
        for (var i = 0; i < 100; i++)
        {
            var v = RandomGenerator.GetInt(10);
            Assert.InRange(v, 0, 9);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(50)]
    public void GetString_ProducesExpectedLength(int length)
    {
        var result = RandomGenerator.GetString(length);

        Assert.Equal(length, result.Length);
    }

    [Theory]
    [InlineData(RandomGenerator.CharOptions.AllLower)]
    [InlineData(RandomGenerator.CharOptions.AllUpper)]
    [InlineData(RandomGenerator.CharOptions.AlphabeticLower)]
    [InlineData(RandomGenerator.CharOptions.AlphabeticUpper)]
    [InlineData(RandomGenerator.CharOptions.Digital)]
    [InlineData(RandomGenerator.CharOptions.HexLower)]
    [InlineData(RandomGenerator.CharOptions.HexUpper)]
    public void GetString_CharsMatchChoice(RandomGenerator.CharOptions option)
    {
        var result = RandomGenerator.GetString(64, option);

        Assert.All(result, ch =>
        {
            Assert.True(IsAllowed(ch, option),
                $"char '{ch}' not allowed for {option}");
        });
    }

    [Fact]
    public void GetString_UnknownOption_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RandomGenerator.GetString(5, (RandomGenerator.CharOptions)999));
    }

    [Fact]
    public void GetItems_ProducesExpectedLength()
    {
        ReadOnlySpan<char> choices = new[] { 'a', 'b', 'c' };

        var result = RandomGenerator.GetItems<char>(choices, 10);

        Assert.Equal(10, result.Length);
        Assert.All(result, ch => Assert.Contains(ch, new[] { 'a', 'b', 'c' }));
    }

    [Fact]
    public void GetItems_EmptyChoices_Throws()
    {
        Assert.Throws<ArgumentException>(() => RandomGenerator.GetItems(ReadOnlySpan<char>.Empty, 5));
    }

    [Fact]
    public void GetItems_LengthOverIntMax_Throws()
    {
        //  checked before the array allocation — must not throw OverflowException/OOM
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RandomGenerator.GetItems<char>(new[] { 'a' }.AsSpan(), int.MaxValue + 1u));
    }

    [Fact]
    public void GetItems_SingleChoice_AllElementsAreThatChoice()
    {
        var result = RandomGenerator.GetItems<char>(new[] { 'z' }.AsSpan(), 1000);

        Assert.Equal(1000, result.Length);
        Assert.All(result, ch => Assert.Equal('z', ch));
    }

    [Fact]
    public void Shuffle_Memory_Works()
    {
        var mem  = new Memory<int>(new[] { 1, 2, 3, 4, 5 });
        var copy = mem.ToArray();

        RandomGenerator.Shuffle(mem);

        Assert.Equal(mem.ToArray().OrderBy(x => x), copy.OrderBy(x => x));
    }

    private static bool IsAllowed(char ch, RandomGenerator.CharOptions option) => option switch
    {
        RandomGenerator.CharOptions.AllLower => RandomGenerator.CharsAllLower.Contains(ch),
        RandomGenerator.CharOptions.AllUpper => RandomGenerator.CharsAllUpper.Contains(ch),
        RandomGenerator.CharOptions.AlphabeticLower => RandomGenerator.CharsAlphabeticLower.Contains(ch),
        RandomGenerator.CharOptions.AlphabeticUpper => RandomGenerator.CharsAlphabeticUpper.Contains(ch),
        RandomGenerator.CharOptions.Digital => RandomGenerator.CharsDigital.Contains(ch),
        RandomGenerator.CharOptions.HexLower => RandomGenerator.CharsHexLower.Contains(ch),
        RandomGenerator.CharOptions.HexUpper => RandomGenerator.CharsHexUpper.Contains(ch),
        _ => false,
    };
}