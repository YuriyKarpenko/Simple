using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Helpers;

public class StrUtilTests
{
    [Theory]
    [InlineData(typeof(List<>), "List<>")]
    [InlineData(typeof(Dictionary<string, List<int>>), "Dictionary<String, List<Int32>>")]
    public void GetClassName(Type type, string expected)
    {
        Assert.Equal(expected, type.GetClassName());
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("x", true)]
    public void NotEmpty(string? value, bool expected)
    {
        Assert.Equal(expected, value.NotEmpty());
    }

    [Theory]
    [InlineData("abc", null, new byte[] { 0x61, 0x62, 0x63 })]
    public void GetBytes_Utf8ByDefault(string value, Encoding? encoding, byte[] expected)
    {
        Assert.Equal(expected, value.GetBytes(encoding));
    }

    [Fact]
    public void GetBytes_WithCustomEncoding()
    {
        Assert.Equal(new byte[] { 0x61, 0x62 }, "ab".GetBytes(Encoding.ASCII));
    }

    [Theory]
    [InlineData("1234567890", 10, "1234567890")]
    [InlineData("1234567890", 15, "1234567890")]
    [InlineData("1234567890", 3, "123")]
    [InlineData("1234567890", 4, "1234")]
    [InlineData("1234567890", 5, "1 ...")]
    [InlineData("1234567890", 6, "12 ...")]
    [InlineData("1234567890", 7, "123 ...")]
    public void TrimLen(string value, uint maxLen, string expected)
    {
        Assert.Equal(expected, value.TrimLen(maxLen));
    }
}