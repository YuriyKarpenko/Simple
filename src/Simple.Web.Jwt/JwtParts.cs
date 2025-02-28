namespace Simple.Web.Jwt;

/// <summary>
/// Represent the parts of a JWT
/// </summary>
public class JwtParts
{
    public static IOption<JwtParts> OptParse(string token, IBase64UrlEncoder base64UrlEncoder)
    {
        var oJwt = Option.String(token)
            .Validate(StrUtil.NotEmpty)
            .ThenValue(i => i.Split('.'))
            .Validate(i => i.Length == 3, JwtErrors.ErrorTokenParse)
            .Validate(ss => StrUtil.NotEmpty(ss[0]), JwtErrors.ErrorArgumentIsInvalid("Header"))
            .Validate(ss => StrUtil.NotEmpty(ss[1]), JwtErrors.ErrorArgumentIsInvalid("Payload"))
            .ThenTryValue(ss =>
            {
                var oHeader = base64UrlEncoder.OptDecode(ss[0]).ThenValue(Utf8Utils.GetString);
                var oPayload = base64UrlEncoder.OptDecode(ss[1]).ThenValue(Utf8Utils.GetString);
                var oSign = ss[2].Length > 0 ? base64UrlEncoder.OptDecode(ss[2]) : Option.Value(new byte[0]);   //  can be empty
                var bytesToSign = Utf8Utils.GetBytesToSign(ss[0], ss[1]);
                return new JwtParts(bytesToSign, oHeader.Value, oPayload.Value, oSign.Value);
            });

        return oJwt;
    }

    public static bool TryParse(string token, IBase64UrlEncoder base64UrlEncoder, out JwtParts jwtParts, out string? e)
    {
        Throw.IsArgumentNullException(token, StrUtil.NotEmpty, nameof(token));

        jwtParts = null!;
        e = null;

        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            e = JwtErrors.ErrorTokenParse;
            return false;
        }

        if (!JwtErrors.IsArgumentNotEmpty(parts[0], StrUtil.NotEmpty, "Header", out e))
        {
            return false;
        }

        if (!JwtErrors.IsArgumentNotEmpty(parts[1], StrUtil.NotEmpty, "Payload", out e))
        {
            return false;
        }

        base64UrlEncoder.TryDecode(parts[2], out var signature, out e);   //  can be empty

        var bytesToSign = Utf8Utils.GetBytesToSign(parts[0], parts[1]);

        try
        {
            if (Decode(base64UrlEncoder, parts[0], out var headerJson, out e) &&
                Decode(base64UrlEncoder, parts[1], out var payloadJson, out e))
            {
                jwtParts = new JwtParts(bytesToSign, headerJson, payloadJson, signature);
                return true;
            }
        }
        catch (Exception ex)
        {
            e = ex.Message;
        }

        return false;

        static bool Decode(IBase64UrlEncoder base64UrlEncoder, string input, out string json, out string? error)
        {
            error = null;
            json = string.IsNullOrEmpty(input)
                ? string.Empty
                : base64UrlEncoder.TryDecode(input, out var bb, out error)
                    ? Utf8Utils.GetString(bb)
                    : string.Empty;

            return error is null;
        }
    }

    public JwtParts(byte[] bytesToSign, string headerJson, string payloadJson, byte[] signature)
    {
        BytesToSign = bytesToSign;
        HeaderJson = headerJson;
        PayloadJson = payloadJson;
        Signature = signature;
    }


    public byte[] BytesToSign { get; }

    /// <summary> Gets the Header part of a JWT as JSON string </summary>
    public string HeaderJson { get; }

    /// <summary> Gets the Payload part of a JWT as JSON string </summary>
    public string PayloadJson { get; }

    /// <summary> Gets the Signature part of a JWT (decoded from Base64) </summary>
    public byte[] Signature { get; }

    public JwtHeader? Header { get; set; }
    public IJwtPayload? Payload { get; set; }
}

public static class JwtPartsExtensions
{
    public static JwtParts EnsureHeader(this JwtParts jwt, IJsonSerializer jsonSerializer)
    {
        jwt.Header ??= jsonSerializer.Deserialize<JwtHeader>(jwt.HeaderJson);
        return jwt;
    }

    public static IJwtPayload EnsurePayload(this JwtParts jwt, IJsonSerializer jsonSerializer)
        => jwt.Payload ??= jsonSerializer.Deserialize<JwtPayload>(jwt.PayloadJson);
}