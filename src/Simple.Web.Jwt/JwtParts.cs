namespace Simple.Web.Jwt;

/// <summary>
/// Represent the parts of a JWT
/// </summary>
public class JwtParts
{
    public static bool TryParse(string token, IBase64UrlEncoder base64UrlEncoder, out JwtParts jwtParts, out Exception? e)
    {
        e = null;

        try
        {
            jwtParts = new JwtParts(token, base64UrlEncoder);
        }
        catch (Exception ex)
        {
            e = ex;
            jwtParts = null!;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Creates a new instance of <see cref="JwtParts" /> from the string representation of a JWT
    /// </summary>
    /// <param name="token">The string representation of a JWT</param>
    /// <exception cref="ArgumentException" />
    /// <exception cref="ArgumentOutOfRangeException" />
    public JwtParts(string token, IBase64UrlEncoder base64UrlEncoder)
    {
        Throw.IsArgumentNullException(token, StrUtil.NotEmpty, nameof(token));

        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            throw new ArgumentOutOfRangeException(JwtErrors.ErrorTokenParse);
        }

        var HeaderRaw = Throw.IsArgumentNullException(parts[0], StrUtil.NotEmpty, "Header");
        var PayloadRaw = Throw.IsArgumentNullException(parts[1], StrUtil.NotEmpty, "Payload");
        //Signature = Throw.IsArgumentNullException(parts[2], NotEmpty, nameof(Signature));
        Signature = base64UrlEncoder.Decode(parts[2]);   //  can be empty

        BytesToSign = Utf8Utils.GetBytesToSign(HeaderRaw, PayloadRaw);

        HeaderJson = Decode(base64UrlEncoder, HeaderRaw);
        PayloadJson = Decode(base64UrlEncoder, PayloadRaw);

        static string Decode(IBase64UrlEncoder base64UrlEncoder, string input)
        {
            var bb = base64UrlEncoder.Decode(input);
            var res = Utf8Utils.GetString(bb);
            return res;
        }
    }

    public byte[] BytesToSign { get; }

    public string HeaderJson { get; }

    /// <summary>
    /// Gets the Payload part of a JWT
    /// </summary>
    public string PayloadJson { get; }

    /// <summary>
    /// Gets the Signature part of a JWT
    /// </summary>
    public byte[] Signature { get; }

    public JwtHeader? Header { get; set; }
    public IJwtPayload? Payload { get; set; }
}