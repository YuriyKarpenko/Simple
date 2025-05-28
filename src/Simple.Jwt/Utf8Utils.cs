using System.Text;

namespace Simple.Jwt;

/// <summary>
/// base64 encoding/decoding implementation according to the JWT spec
/// </summary>
public static class Utf8Utils
{
    private static readonly UTF8Encoding _utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public static string FirstSegment(this string input, char separator)
    {
        var idx = input.IndexOf(separator);
        return idx == -1 ? input : input.Substring(0, idx);
    }

    public static byte[] GetBytes(this string input)
        => _utf8.GetBytes(input);

    public static byte[] GetBytesToSign(string headerRaw, string payloadRaw)
    {
        var output = new byte[_utf8.GetByteCount(headerRaw) + _utf8.GetByteCount(payloadRaw) + 1];
        var bytesWritten = _utf8.GetBytes(headerRaw, 0, headerRaw.Length, output, 0);
        output[bytesWritten++] = (byte)'.';
        _utf8.GetBytes(payloadRaw, 0, payloadRaw.Length, output, bytesWritten);
        return output;
    }

    public static string GetString(byte[] input)
        => _utf8.GetString(input);

    public static IOption<R> ThenValue<T, R>(this IOption<T> o, Func<T, R> select)
        => o.Then(i => Option.Value(select(o.Value)));
}