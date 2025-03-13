namespace Test.Helpers;

public class HexTests
{
    [Theory]
    [InlineData(4, 0)]
    [InlineData(14, 0)]
    [InlineData(24, 1)]
    [InlineData(0x24, 2)]
    [InlineData(0xF4, 15)]
    public void HiByte(byte value, byte expected)
    {
        var actual = value.HiByte();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(4, 4)]
    [InlineData(14, 14)]
    [InlineData(24, 8)]
    public void LoByte(byte value, byte expected)
    {
        var actual = value.LoByte();

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0x24, false, "24")]
    [InlineData(0xE7, false, "E7")]
    [InlineData(0xE7, true, "e7")]
    public void ToHexStr(byte value, bool isLowerCase, string expected)
    {
        var actual = value.ToHexStr(isLowerCase);

        Assert.Equal(expected, actual);
    }
}