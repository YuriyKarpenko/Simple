using System;
using System.Collections.Generic;

namespace Test.Helpers;

public class ExtensionsHexTests
{
    [Fact]
    public void ToHexStr_ForByte_Works_InUpperAndLowerCase()
    {
        const byte value = 0xAF;

        Assert.Equal("AF", value.ToHexStr());
        Assert.Equal("af", value.ToHexStr(true));
    }

    [Fact]
    public void ToHexString_ForEnumerable_WithoutSeparator_UsesFastPath()
    {
        IEnumerable<byte> bytes = [0x01, 0x02, 0x03];

        var result = bytes.ToHexString();

        Assert.Equal("010203", result);
    }

    [Fact]
    public void ToHexString_ForEnumerable_WithSeparator_Works()
    {
        IEnumerable<byte> bytes = [0x0A, 0x0B];

        var result = bytes.ToHexString(true, ":");

        Assert.Equal("0a:0b", result);
    }

    [Fact]
    public void ToHexChars_ToHexChar_HiByte_And_LoByte_Work()
    {
        const byte value = 0xAB;

        Assert.Equal(new[] { 'A', 'B' }, value.ToHexChars());
        Assert.Equal('c', 12.ToHexChar(true));
        Assert.Equal((byte)0x0A, value.HiByte());
        Assert.Equal((byte)0x0B, value.LoByte());
    }

    [Fact]
    public void ToHexChar_InvalidValue_Throws()
    {
        Assert.Throws<IndexOutOfRangeException>(() => ((byte)16).ToHexChar());
    }

    [Fact]
    public void ToHexString_Empty_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, Array.Empty<byte>().ToHexString());
    }

    [Fact]
    public void IntOverloads_HiByte_LoByte_Work()
    {
        Assert.Equal((byte)0x0A, 0xAB.HiByte());
        Assert.Equal((byte)0x0B, 0xAB.LoByte());
        Assert.Equal((byte)0x0F, 255.LoByte());
    }

    [Fact]
    public void IntOverloads_OutOfRange_Throws()
    {
        Assert.Throws<IndexOutOfRangeException>(() => 256.HiByte());
        Assert.Throws<IndexOutOfRangeException>(() => (-1).HiByte());
        Assert.Throws<IndexOutOfRangeException>(() => 256.LoByte());
        Assert.Throws<IndexOutOfRangeException>(() => (-1).LoByte());
        Assert.Throws<IndexOutOfRangeException>(() => 16.ToHexChar());
        Assert.Throws<IndexOutOfRangeException>(() => (-1).ToHexChar());
    }

    [Fact]
    public void GetServiceRequired_ReturnsService_AndThrowsWhenMissing()
    {
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(string)).Returns("hello");

        Assert.Equal("hello", sp.GetServiceRequired<string>());
        Assert.Equal("hello", sp.GetServiceRequired(typeof(string)));
        Assert.Throws<ArgumentNullException>(() => sp.GetServiceRequired(typeof(int)));
    }

    [Fact]
    public void GetServiceRequired_NullProviderOrType_Throws()
    {
        var sp = Substitute.For<IServiceProvider>();

        Assert.Throws<ArgumentNullException>(() => ((IServiceProvider)null!).GetServiceRequired(typeof(string)));
        Assert.Throws<ArgumentNullException>(() => sp.GetServiceRequired((Type)null!));
    }
}