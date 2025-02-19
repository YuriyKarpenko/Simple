namespace Simple.Web.Jwt;

/// <summary> Represents a base64 encoder/decoder. </summary>
public interface IBase64UrlEncoder
{
    /// <summary> Encodes the byte array to a base64 string. </summary>
    IOption<string> OptEncode(byte[] input);

    /// <summary> Decodes the base64 string to a byte array. </summary>
    IOption<byte[]> OptDecode(string input);


    /// <summary> Encodes the byte array to a base64 string. </summary>
    bool TryEncode(byte[] input, out string output, out string? error);

    /// <summary> Decodes the base64 string to a byte array. </summary>
    bool TryDecode(string input, out byte[] output, out string? error);
}

//  TODO: use Span
public class Base64UrlEncoder : IBase64UrlEncoder
{

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException" />
    public IOption<string> OptEncode(byte[] input)
    {
        return Option.Value(input)
            .Validate(i => i.Length > 0, JwtErrors.ErrorArgumentIsInvalid(nameof(input)))
            .ThenTryValue(EncodeNew);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="FormatException" />
    public IOption<byte[]> OptDecode(string input)
    {
        return Option.Value(input)
            .Validate(StrUtil.NotEmpty, JwtErrors.ErrorArgumentIsInvalid(nameof(input)))
            .ThenTryValue(DecodeNew);
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException" />
    public bool TryEncode(byte[] input, out string output, out string? error)
    {
        if (JwtErrors.IsArgumentNotEmpty(input, i => i.Length > 0, nameof(input), out error))
        {
            output = Convert.ToBase64String(input);
            output = output.FirstSegment('='); // Remove any trailing '='s
            output = output.Replace('+', '-'); // 62nd char of encoding
            output = output.Replace('/', '_'); // 63rd char of encoding
            return true;
        }

        output = null!;
        return false;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException" />
    /// <exception cref="FormatException" />
    public bool TryDecode(string input, out byte[] output, out string? error)
    {
        if (JwtErrors.IsArgumentNotEmpty(input, StrUtil.NotEmpty, nameof(input), out error))
        {

            input = input.Replace('-', '+'); // 62nd char of encoding
            input = input.Replace('_', '/'); // 63rd char of encoding
            switch (input.Length % 4) // Pad with trailing '='s
            {
                case 0:
                    break; // No pad chars in this case
                case 2:
                    input += "==";
                    break; // Two pad chars
                case 3:
                    input += "=";
                    break; // One pad char
                default:
                    error = JwtErrors.ErrorTokenUrlDecode;
                    output = null!;
                    return false;
            }
            output = Convert.FromBase64String(input); // Standard base64 decoder
            return true;
        }

        output = null!;
        return false;
    }


    private static string EncodeNew(byte[] input)
    {
        var outSize = CalcEncodeResult(input.Length);
        var output = new char[outSize];
        var used = Convert.ToBase64CharArray(input, 0, input.Length, output, 0);
        //  after NetStandard2.1
        //Span<char> output = stackalloc char[outSize];
        //Convert.TryToBase64Chars(input, output, out var used);
        for (var i = 0; i < used; i++)
        {
            var c = output[i];
            switch (c)
            {
                case '+':
                    output[i] = '-';
                    break;
                case '/':
                    output[i] = '_';
                    break;
                case '=':
                    used = i;
                    break;
            }
        }

        return new string(output, 0, used);
    }

    private static byte[] DecodeNew(string input)
    {
        var appendSize = (4 - input.Length % 4) % 4;
        //if (appendSize == 1)
        //{
        //    Throw.Exception(new Exception(JwtErrors.ErrorTokenUrlDecode));
        //}

        var inputSpan = new char[input.Length + appendSize];
        for (var i = 0; i < input.Length; i++)
        {
            var ci = input[i];
            inputSpan[i] = ci switch
            {
                '-' => '+',
                '_' => '/',
                char c => c,
            };
        }

        while (appendSize-- > 0)
        {
            inputSpan[input.Length + appendSize] = '=';
        }

        return Convert.FromBase64CharArray(inputSpan, 0, inputSpan.Length);
    }

    private static int CalcEncodeResult(int inputLength)
        => (inputLength + 2) / 3 * 4;
}