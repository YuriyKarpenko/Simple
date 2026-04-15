using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Buffers;

namespace Simple.Helpers;

public class JsonConverterBytesHex : JsonConverter<byte[]?>
{
    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            Throw.Exception(new JsonException($"Expected {JsonTokenType.String}, but got {reader.TokenType}."));
        }

        if (reader.HasValueSequence)
        {
            var rented = reader.ValueSequence.ToArray();
            try
            {
                return rented.Length == 0 ? [] : HexConverter.AsBytes(rented);
            }
            catch (Exception ex) when (ex is not JsonException)
            {
                Throw.Exception(new JsonException("Invalid hex payload.", ex));
            }
        }

        var span = reader.ValueSpan;

        try
        {
            return span.Length == 0 ? [] : HexConverter.AsBytes(span);
        }
        catch (Exception ex) when (ex is not JsonException)
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