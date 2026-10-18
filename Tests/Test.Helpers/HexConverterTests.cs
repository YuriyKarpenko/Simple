using System;
using System.Text;

namespace Test.Helpers;

public class HexConverterTests
{
    [Fact]
    public void AsString_Uppercase_RoundTrips()
    {
        var source = new byte[] { 0x01, 0xAB, 0xFF };

        var hex = HexConverter.AsString(source);
        var bytes = HexConverter.AsBytes(hex.GetBytes());

        Assert.Equal("01ABFF", hex);
        Assert.Equal(source, bytes);
    }

    [Fact]
    public void AsString_Lowercase_Works()
    {
        var source = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };

        var hex = HexConverter.AsString(source, true);

        Assert.Equal("deadbeef", hex);
    }

    [Fact]
    public void AsBytes_FromString_Works()
    {
        var bytes = HexConverter.AsBytes("0A10ff");

        Assert.Equal(new byte[] { 0x0A, 0x10, 0xFF }, bytes);
    }

    [Fact]
    public void AsBytes_OddLength_Throws()
    {
        var ex = Assert.Throws<Exception>(() => HexConverter.AsBytes("ABC"));
        Assert.Contains("even length", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AsBytes_InvalidChar_Throws()
    {
        var ex = Assert.Throws<Exception>(() => HexConverter.AsBytes("GG"));
        Assert.Contains("Invalid hex", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AsChars_Matches_AsString()
    {
        var source = new byte[] { 0x12, 0x34 };

        var chars = HexConverter.AsChars(source);

        Assert.Equal("1234", new string(chars));
    }
}