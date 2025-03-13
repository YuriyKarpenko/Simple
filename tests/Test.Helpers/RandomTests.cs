using System;
using System.Linq;

namespace Test.Helpers;

public class RandomTests
{
    [Theory]
    [InlineData(10)]
    public void Fill(int len)
    {
        var bb = new byte[len];

        RandomGenerator.Fill(bb);

        Assert.Equal(0, bb.Count(i => i == 0));
    }

    [Theory]
    [InlineData(10)]
    public void Shuffle(int len)
    {
        //  arrange
        Span<byte> span = stackalloc byte[len];
        RandomGenerator.Fill(span);
        var bb = span.ToArray();

        //  test
        RandomGenerator.Shuffle(span);

        //  assert
        Assert.NotEqual(span.ToArray(), bb);
    }
}