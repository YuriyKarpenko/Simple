namespace Simple.Web.Jwt;

/// <summary>
/// JSON header model with predefined parameter names specified by RFC 7515, see https://tools.ietf.org/html/rfc7515
/// </summary>
public class JwtHeader
{
    public const string JwtType = "JWT";

    //  https://datatracker.ietf.org/doc/html/rfc7519#section-5.1
    /// <summary> Type </summary>
    public string? typ { get; set; }

    ////  https://datatracker.ietf.org/doc/html/rfc7519#section-5.2
    ///// <summary> ContentType </summary>
    //public string? cty { get; set; }

    /// <summary> Algorithm </summary>
    public string? alg { get; set; }

    /// <summary> KeyId </summary>
    public string? kid { get; set; }

    //public string? x5u { get; set; }

    //public string[]? x5c { get; set; }

    //public string? x5t { get; set; }
}