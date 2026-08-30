using System;
using System.Text.Json;

namespace Test.Helpers;

public class JsonConverterBytesBase64Tests
{
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonConverterBytesBase64());
        return options;
    }

    [Fact]
    public void Serialize_And_Deserialize_RoundTrip()
    {
        var options = CreateOptions();
        var source  = new byte[] { 0x01, 0x02, 0xFF };

        var json  = JsonSerializer.Serialize(source, options);
        var value = JsonSerializer.Deserialize<byte[]?>(json, options);

        Assert.Equal("\"AQL/\"", json);
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
    public void Serialize_Null_WritesNull()
    {
        var options = CreateOptions();

        var json = JsonSerializer.Serialize<byte[]?>(null, options);

        Assert.Equal("null", json);
    }

    [Fact]
    public void Deserialize_NonString_Throws()
    {
        var options = CreateOptions();

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<byte[]?>("123", options));
    }

    [Fact]
    public void Deserialize_InvalidBase64_Throws()
    {
        var options = CreateOptions();

        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<byte[]?>("\"not base64!!\"", options));
    }

    [Fact]
    public void Write_NullValue_WritesJsonNull()
    {
        var converter = new JsonConverterBytesBase64();
        var buffer    = new System.Buffers.ArrayBufferWriter<byte>();

        using (var writer = new Utf8JsonWriter(buffer))
        {
            converter.Write(writer, null, CreateOptions());
            writer.Flush();
        }

        Assert.Equal("null", System.Text.Encoding.UTF8.GetString(buffer.WrittenSpan));
    }
}