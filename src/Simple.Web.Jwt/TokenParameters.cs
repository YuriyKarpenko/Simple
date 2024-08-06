using Simple.Web.Jwt.Algorithms;

namespace Simple.Web.Jwt;

/// <summary> Contains a set of parameters that are used in the processing of token. </summary>
public class TokenParameters
{
    #region static

    /// <summary> Returns a <see cref="TokenParameters" /> with all properties set to <see langword="true" />. </summary>
    public static TokenParameters Default => new TokenParameters
    {
        ValidateExpirationTime = true,
        ValidateIssuedTime = true,
        ValidateSignature = true,
    };

    /// <summary> Returns a <see cref="TokenParameters" /> with all properties set to <see langword="false" />. </summary>
    public static TokenParameters None => new TokenParameters();

    #endregion

    public TokenParameters(IBase64UrlEncoder? urlEncoder = null, IDateTimeProvider? timeProvider = null, IJsonSerializer? jsonSerializer = null)
    {
        UrlEncoder = urlEncoder ?? new Base64UrlEncoder();
        DateTimeProvider = timeProvider ?? new UtcDateTimeProvider();
        JsonSerializer = jsonSerializer = new NewtonsoftSerializer();

        Payload = new JwtPayload();
        TimeMargin = TimeSpan.FromMinutes(1);
    }
    //public TokenParameters(IServiceProvider sp) : this(sp.GetService<IBase64UrlEncoder>()), sp.GetService<IDateTimeProvider>(), sp.GetService<IJsonSerializer>()) { }

    public IJwtAlgorithm? Algorithm { get; set; }
    public IBase64UrlEncoder UrlEncoder { get; set; }
    public IDateTimeProvider DateTimeProvider { get; set; }
    public IJsonSerializer JsonSerializer { get; set; }
    public IJwtPayload Payload { get; }

    /// <summary> Gets or sets whether to validate the validity of the token's signature. </summary>
    public bool ValidateSignature { get; set; }
    public byte[]? SigningKey { get; set; }

    /// <summary> Gets or sets whether to validate the validity of the token's issued time. </summary>
    public bool ValidateIssuedTime { get; set; }
    public bool ValidateIssuer { get; set; }
    public string ValidIssuer
    {
        set => Payload[ClaimName.Issuer] = value;
    }

    public bool ValidateAudience { get; set; }
    public string ValidAudience
    {
        set => Payload[ClaimName.Audience] = value;
    }

    /// <summary> Gets or sets whether to validate the validity of the token's expiration time. </summary>
    public bool ValidateExpirationTime { get; set; }
    public TimeSpan? ValidExpiration { get; set; }


    /// <summary> Gets or sets the time margin in seconds for exp and nbf during token validation. </summary>
    public TimeSpan TimeMargin { get; set; }


    public bool Validate(out string errorMessage)
    {
        errorMessage = Algorithm is null
            ? JwtErrors.ErrorArgumentIsNull(nameof(Algorithm))
            : JsonSerializer is null
                ? JwtErrors.ErrorArgumentIsNull(nameof(JsonSerializer))
                : UrlEncoder is null
                    ? JwtErrors.ErrorArgumentIsNull(nameof(UrlEncoder))
                    : (ValidateExpirationTime || ValidateIssuedTime) && DateTimeProvider is null
                        ? JwtErrors.ErrorArgumentIsNull(nameof(DateTimeProvider))
                        : string.Empty;

        return errorMessage.Length == 0;
    }
}