using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Simple.Helpers;

public class JsonConverterBytesBase64 : JsonConverter<byte[]?>
{
    //  note: JSON escapes are NOT decoded — the raw UTF-8 payload is base64-decoded;
    //  escaped content is not valid base64 and throws (FormatException wrapped as JsonException)
    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            Throw.Exception(new JsonException($"Expected {JsonTokenType.String}, but got {reader.TokenType}."));
        }

        try
        {
            return reader.GetBytesFromBase64();
        }
        catch (FormatException ex)
        {
            Throw.Exception(new JsonException("Invalid Base64 payload.", ex));
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, byte[]? bytes, JsonSerializerOptions options)
    {
        if (bytes == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteBase64StringValue(bytes);
    }
}