using System;
using System.Text.Json;

namespace Test.Helpers;

public class JsonConverterBytesHexTests
{
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonConverterBytesHex());
        return options;
    }

    [Fact]
    public void Serialize_And_Deserialize_RoundTrip()
    {
        var options = CreateOptions();
        var source = new byte[] { 0x10, 0x20, 0xFF };

        var json = JsonSerializer.Serialize(source, options);
        var value = JsonSerializer.Deserialize<byte[]?>(json, options);

        Assert.Equal("\"1020FF\"", json);
        Assert.Equal(source, value);
    }

    [Fact]
    public void Deserialize_Null_ReturnsNull()
    {
        var options = CreateOptions();

        var value = JsonSerializer.Deserialize<byte[]?>("null", options);

        Assert.Null(value);
    }

    [Fact]
    public void Deserialize_EmptyString_ReturnsNull()
    {
        var options = CreateOptions();

        var value = JsonSerializer.Deserialize<byte[]?>("\"\"", options);

        Assert.Equal([], value);
    }

    [Fact]
    public void Deserialize_InvalidHex_Throws()
    {
        var options = CreateOptions();

        Assert.ThrowsAny<Exception>(() => JsonSerializer.Deserialize<byte[]?>("\"GG\"", options));
    }
}