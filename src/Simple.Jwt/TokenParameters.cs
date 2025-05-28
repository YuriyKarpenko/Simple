using Simple.Jwt.Algorithms;

namespace Simple.Jwt;

/// <summary> Contains a set of parameters that are used in the processing of token. </summary>
public class TokenParameters
{
    public const string JwtSchemeName = "Bearer";

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
        get => Payload.TryGet(ClaimName.Issuer, out string v) ? v : string.Empty;
        set => Payload[ClaimName.Issuer] = value;
    }

    public bool ValidateAudience { get; set; }
    public string ValidAudience
    {
        get => Payload.TryGet(ClaimName.Audience, out string v) ? v : string.Empty;
        set => Payload[ClaimName.Audience] = value;
    }

    /// <summary> Gets or sets whether to validate the validity of the token's expiration time. </summary>
    public bool ValidateExpirationTime { get; set; }
    public TimeSpan? ValidExpiration { get; set; }


    /// <summary> Gets or sets the time margin in seconds for exp and nbf during token validation. </summary>
    public TimeSpan TimeMargin { get; set; }


    public bool Validate(out string? errorMessage)
    {
        errorMessage = Algorithm is null
            ? JwtErrors.ErrorArgumentIsNull(nameof(Algorithm))
            : JsonSerializer is null
                ? JwtErrors.ErrorArgumentIsNull(nameof(JsonSerializer))
                : UrlEncoder is null
                    ? JwtErrors.ErrorArgumentIsNull(nameof(UrlEncoder))
                    : (ValidateExpirationTime || ValidateIssuedTime) && DateTimeProvider is null
                        ? JwtErrors.ErrorArgumentIsNull(nameof(DateTimeProvider))
                        : null;

        errorMessage ??= ValidateAudience && string.IsNullOrEmpty(ValidAudience) ? JwtErrors.ErrorArgumentIsInvalid(nameof(ValidAudience)) : null;
        errorMessage ??= ValidateExpirationTime && !ValidExpiration.HasValue ? JwtErrors.ErrorArgumentIsInvalid(nameof(ValidExpiration)) : null;
        errorMessage ??= ValidateIssuer && string.IsNullOrEmpty(ValidIssuer) ? JwtErrors.ErrorArgumentIsInvalid(nameof(ValidIssuer)) : null;

        return errorMessage is null;
    }
}

public static class ParametersExtensions
{
    //  Decode

    public static IOption<IJwtValidator> OptValidator(this TokenParameters _parameters)
    {
        Throw.IsArgumentNullException(_parameters, nameof(_parameters));

        return _parameters.Validate(out var error)
            ? Option.Value(new JwtValidator(_parameters))
            : Option.Error<IJwtValidator>(error);
    }

    public static IOption<IJwtPayload> OptValidateToken(this TokenParameters _parameters, string token)
    {
        return OptValidator(_parameters)
            .Join(JwtParts.OptParse(token, _parameters.UrlEncoder), (v, p) =>
            {
                var error = v.Validate(p);
                return error is null
                    ? Option.Value(p.EnsurePayload(_parameters.JsonSerializer))
                    : Option.Error<IJwtPayload>(error);
            });
    }



    public static bool TryCreateValidator(this TokenParameters _parameters, out IJwtValidator jwtValidator, out string? error)
    {
        Throw.IsArgumentNullException(_parameters, nameof(_parameters));

        if (_parameters.Validate(out error))
        {
            jwtValidator = new JwtValidator(_parameters);
            return true;
        }

        jwtValidator = null!;
        return false;
    }

    public static bool TryValidateToken(this TokenParameters _parameters, string token, out IDictionary<string, object> payload, out string? error)
    {
        if (TryCreateValidator(_parameters, out var validator, out error) &&
            JwtParts.TryParse(token, _parameters.UrlEncoder, out var jwt, out error) &&
            validator.TryValidate(jwt, out error))
        {
            payload = jwt.EnsurePayload(_parameters.JsonSerializer);
            return true;
        }

        payload = null!;
        return false;
    }

    //  Encode
    public static IOption<IJwtEncoder> OptEncoder(this TokenParameters _parameters)
    {
        Throw.IsArgumentNullException(_parameters, nameof(_parameters));

        return _parameters.Validate(out var error)
            ? Option.Value(new JwtEncoder(_parameters.Algorithm!, _parameters.JsonSerializer, _parameters.UrlEncoder))
            : Option.Error<IJwtEncoder>(error);
    }

    public static IOption<string> OptToken(this TokenParameters _parameters, IDictionary<string, object>? claims)
    {
        return OptEncoder(_parameters)
            .Then(en =>
            {
                var payload = MergePayload(_parameters, claims);
                return en.OptEncode(payload, _parameters.SigningKey);
            });
    }


    public static bool TryCreateEncoder(this TokenParameters _parameters, out IJwtEncoder encoder, out string? error)
    {
        _parameters = Throw.IsArgumentNullException(_parameters, nameof(_parameters));
        if (_parameters.Validate(out error))
        {
            encoder = new JwtEncoder(_parameters.Algorithm!, _parameters.JsonSerializer, _parameters.UrlEncoder);
            return true;
        }

        encoder = null!;
        return false;
    }

    public static bool TryCreateToken(this TokenParameters _parameters, IDictionary<string, object>? claims, out string token, out string? error)
    {
        if (_parameters.TryCreateEncoder(out var encoder, out error))
        {
            var payload = MergePayload(_parameters, claims);
            return encoder.TryEncode(payload, _parameters.SigningKey, out token, out error);
        }

        token = null!;
        return false;
    }


    private static IJwtPayload MergePayload(TokenParameters _parameters, IDictionary<string, object>? claims)
    {
        var payload = new JwtPayload(_parameters.Payload);

        if (_parameters.ValidateExpirationTime || _parameters.ValidateIssuedTime)
        {
            var now = _parameters.DateTimeProvider.GetNow();

            if (_parameters.ValidateIssuedTime)
            {
                var nb = UnixEpoch.GetSecondsSince(now);
                payload.NotBefore(nb);
                payload.IssuedAt(nb);
            }

            if (_parameters.ValidateExpirationTime && _parameters.ValidExpiration.HasValue)
            {
                var exp = now + _parameters.ValidExpiration.Value;
                payload.ExpirationTime(UnixEpoch.GetSecondsSince(exp));
            }
        }

        if (claims is not null)
        {
            payload.Merge(claims);
        }

        return payload;
    }
}