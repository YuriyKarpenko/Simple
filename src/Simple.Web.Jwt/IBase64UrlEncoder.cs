namespace Simple.Web.Jwt;

/// <summary> Represents a base64 encoder/decoder. </summary>
public interface IBase64UrlEncoder
{
    /// <summary> Encodes the byte array to a base64 string. </summary>
    string Encode(byte[] input);

    /// <summary> Decodes the base64 string to a byte array. </summary>
    byte[] Decode(string input);
}

//  TODO: use Span
public class Base64UrlEncoder : IBase64UrlEncoder
{
    /// <inheritdoc />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="ArgumentOutOfRangeException" />
    public string Encode(byte[] input)
    {
        Throw.IsArgumentNullException(input, i => i.Length > 0, nameof(input));

        var output = Convert.ToBase64String(input);
        output = output.FirstSegment('='); // Remove any trailing '='s
        output = output.Replace('+', '-'); // 62nd char of encoding
        output = output.Replace('/', '_'); // 63rd char of encoding
        return output;
    }


    /// <inheritdoc />
    /// <exception cref="ArgumentException" />
    /// <exception cref="FormatException" />
    public byte[] Decode(string input)
    {
        Throw.IsArgumentNullException(input, StrUtil.NotEmpty, nameof(input));

        var output = input;
        output = output.Replace('-', '+'); // 62nd char of encoding
        output = output.Replace('_', '/'); // 63rd char of encoding
        switch (output.Length % 4) // Pad with trailing '='s
        {
            case 0:
                break; // No pad chars in this case
            case 2:
                output += "==";
                break; // Two pad chars
            case 3:
                output += "=";
                break; // One pad char
            default:
                throw new FormatException("Illegal base64url string.");
        }
        var converted = Convert.FromBase64String(output); // Standard base64 decoder
        return converted;
    }
}