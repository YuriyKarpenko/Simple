using System;
using System.Buffers;
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
        var source  = new byte[] { 0x10, 0x20, 0xFF };

        var json  = JsonSerializer.Serialize(source, options);
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

        Assert.Equal(Array.Empty<byte>(), value);
    }

    [Fact]
    public void Deserialize_InvalidHex_Throws()
    {
        var options = CreateOptions();

        Assert.ThrowsAny<Exception>(() => JsonSerializer.Deserialize<byte[]?>("\"GG\"", options));
    }

    [Fact]
    public void Deserialize_EscapedPayload_Throws()
    {
        var options = CreateOptions();

        //  escapes are NOT decoded: "\00310032" would be valid hex "12" after decoding, but raw payload is not hex
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<byte[]?>("\"\\u0031\\u0032\"", options));
    }

    [Fact]
    public void Read_SegmentedUtf8String_ReadsHex()
    {
        var options = CreateOptions();
        ReadOnlySpan<byte> json = "\"1020FF\""u8;

        var head = new Seg(json[..5].ToArray());
        var tail = head.Append(json[5..].ToArray());
        var sequence = new ReadOnlySequence<byte>(head, 0, tail, tail.Memory.Length);

        var reader = new Utf8JsonReader(sequence);
        Assert.True(reader.Read());
        Assert.True(reader.HasValueSequence);

        var converter = new JsonConverterBytesHex();
        var value     = converter.Read(ref reader, typeof(byte[]), options);

        Assert.Equal(new byte[] { 0x10, 0x20, 0xFF }, value);
    }

    private static Utf8JsonReader CreateSegmentedReader(string json)
    {
        ReadOnlySpan<byte> bytes = System.Text.Encoding.UTF8.GetBytes(json);

        var head = new Seg(bytes[..1].ToArray());
        var tail = head.Append(bytes[1..].ToArray());
        var sequence = new ReadOnlySequence<byte>(head, 0, tail, tail.Memory.Length);

        var reader = new Utf8JsonReader(sequence);
        Assert.True(reader.Read());
        Assert.True(reader.HasValueSequence);
        return reader;
    }

    [Fact]
    public void Deserialize_NonStringToken_Throws()
    {
        var converter = new JsonConverterBytesHex();
        var reader    = new Utf8JsonReader("123"u8);
        reader.Read();

        var options = CreateOptions();
        var threw   = false;
        try
        {
            converter.Read(ref reader, typeof(byte[]), options);
        }
        catch (JsonException)
        {
            threw = true;
        }

        Assert.True(threw);
    }

    [Fact]
    public void Deserialize_SegmentedEmptyString_ReturnsEmptyArray()
    {
        var converter = new JsonConverterBytesHex();
        var reader    = CreateSegmentedReader("\"\"");

        Assert.Equal(Array.Empty<byte>(), converter.Read(ref reader, typeof(byte[]), CreateOptions()));
    }

    [Fact]
    public void Deserialize_SegmentedInvalidHex_Throws()
    {
        var converter = new JsonConverterBytesHex();
        var reader    = CreateSegmentedReader("\"GG\"");

        var options = CreateOptions();
        var threw   = false;
        try
        {
            converter.Read(ref reader, typeof(byte[]), options);
        }
        catch (JsonException)
        {
            threw = true;
        }

        Assert.True(threw);
    }

    [Fact]
    public void Write_NullValue_WritesJsonNull()
    {
        var converter = new JsonConverterBytesHex();
        var buffer    = new ArrayBufferWriter<byte>();

        using (var writer = new Utf8JsonWriter(buffer))
        {
            converter.Write(writer, null, CreateOptions());
            writer.Flush();
        }

        Assert.Equal("null", System.Text.Encoding.UTF8.GetString(buffer.WrittenSpan));
    }

    private sealed class Seg : ReadOnlySequenceSegment<byte>
    {
        public Seg(ReadOnlyMemory<byte> memory) => Memory = memory;

        public Seg Append(ReadOnlyMemory<byte> memory)
        {
            var next = new Seg(memory) { RunningIndex = RunningIndex + Memory.Length };
            Next     = next;
            return next;
        }
    }
}