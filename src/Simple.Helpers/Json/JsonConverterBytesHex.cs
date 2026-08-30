using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Buffers;

namespace Simple.Helpers;

public class JsonConverterBytesHex : JsonConverter<byte[]?>
{
    //  note: JSON escapes are NOT decoded — the raw UTF-8 payload is parsed as hex;
    //  escaped content (e.g. "A") contains non-hex bytes and throws (FormatException wrapped as JsonException)
    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            Throw.Exception(new JsonException($"Expected {JsonTokenType.String}, but got {reader.TokenType}."));
        }

        if (reader.HasValueSequence)
        {
            var sequence = reader.ValueSequence;
            var length   = checked((int)sequence.Length);

            if (length == 0)
            {
                return [];
            }

            var rented = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                sequence.CopyTo(rented);
                return HexConverter.AsBytes(rented.AsSpan(0, length));
            }
            catch (FormatException ex)
            {
                Throw.Exception(new JsonException("Invalid hex payload.", ex));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }

        var span = reader.ValueSpan;

        try
        {
            return span.Length == 0 ? [] : HexConverter.AsBytes(span);
        }
        catch (FormatException ex)
        {
            Throw.Exception(new JsonException("Invalid hex payload.", ex));
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        var result = HexConverter.AsString(value);

        writer.WriteStringValue(result);
    }
}