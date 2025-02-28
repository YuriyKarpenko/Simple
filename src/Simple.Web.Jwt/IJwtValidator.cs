﻿using System.Linq;

using Simple.Web.Jwt.Algorithms;

namespace Simple.Web.Jwt;

/// <summary>
/// Represents a JWT validator.
/// </summary>
public interface IJwtValidator
{
    /// <summary>
    /// Given the JWT, verifies its signature correctness without throwing an exception but returning it instead.
    /// </summary>
    /// <param name="jwt">The JWT</param>
    /// <returns>Error message if validation fail</returns>
    string? Validate(JwtParts jwt);

    /// <summary>
    /// Given the JWT, verifies its signature correctness without throwing an exception but returning it instead.
    /// </summary>
    /// <param name="jwt">The JWT</param>
    /// <param name="error">The resulting validation error, if any</param>
    /// <returns>Returns <c>true</c> if exception is JWT is valid and exception is null, otherwise false</returns>
    bool TryValidate(JwtParts jwt, out string? error);
}


/// <summary>
/// Jwt validator.
/// </summary>
public sealed class JwtValidator : IJwtValidator
{
    #region static

    /// <summary> Verifies the 'iss' claim. </summary>
    /// <remarks>See https://tools.ietf.org/html/rfc7519#section-4.1.1</remarks>
    public static string? CheckClaimIss(IJwtPayload payloadData, string origValue)
    {
        return !payloadData.TryGet(ClaimName.Issuer, out string value) || !string.Equals(value, origValue)
            ? JwtErrors.ErrorTimeClaim(ClaimName.Issuer)
            : null;
    }

    /// <summary> Verifies the 'aud' claim. </summary>
    /// <remarks>See https://tools.ietf.org/html/rfc7519#section-4.1.3</remarks>
    public static string? CheckClaimAud(IJwtPayload payloadData, string origValue)
    {
        return !payloadData.TryGet(ClaimName.Audience, out string value) || !string.Equals(value, origValue)
            ? JwtErrors.ErrorTimeClaim(ClaimName.Audience)
            : null;
    }

    /// <summary> Verifies the 'exp' claim. </summary>
    /// <remarks>See https://tools.ietf.org/html/rfc7519#section-4.1.4</remarks>
    public static string? CheckClaimExp(IJwtPayload payloadData, long secondsSinceEpoch, TimeSpan timeMargin)
    {
        return payloadData.TryGet(ClaimName.ExpirationTime, out long value)
            ? secondsSinceEpoch - timeMargin.Seconds >= value
                ? JwtErrors.ErrorInvalidClaimExp
                : null
            : JwtErrors.ErrorTimeClaim(ClaimName.ExpirationTime);
    }

    /// <summary> Verifies the 'nbf' claim. </summary>
    /// <remarks>See https://tools.ietf.org/html/rfc7519#section-4.1.5</remarks>
    public static string? CheckClaimNbf(IJwtPayload payloadData, long secondsSinceEpoch, TimeSpan timeMargin)
    {
        return payloadData.TryGet(ClaimName.NotBefore, out long nbfValue)
            ? secondsSinceEpoch + timeMargin.Seconds < nbfValue
                ? JwtErrors.ErrorInvalidClaimNbf
                : null
            : JwtErrors.ErrorTimeClaim(ClaimName.NotBefore);
    }

    /// <summary> Verifies the 'iat' claim. </summary>
    /// <remarks>See https://tools.ietf.org/html/rfc7519#section-4.1.6</remarks>
    public static string? CheckClaimIat(IJwtPayload payloadData, long secondsSinceEpoch, TimeSpan timeMargin)
    {
        return payloadData.TryGet(ClaimName.IssuedAt, out long nbfValue)
            ? secondsSinceEpoch + timeMargin.Seconds < nbfValue
                ? JwtErrors.ErrorInvalidClaimNbf
                : null
            : JwtErrors.ErrorTimeClaim(ClaimName.IssuedAt);
    }

    public static string? CheckNoneAlgorithm(JwtParts jwtWithHeader)
    {
        const string propName = "Header.typ";
        var header = jwtWithHeader.Header!;

        return string.IsNullOrEmpty(header.typ)
            ? JwtErrors.ErrorArgumentIsNull(propName)
            : string.Equals(header.alg, nameof(JwtAlgorithmName.None), StringComparison.OrdinalIgnoreCase) && jwtWithHeader.Signature.Length > 0
                ? JwtErrors.ErrorInvalidNoSign
                : null;
    }

    public static string? CheckSign(JwtParts jwt, IAsymmetricAlgorithm alg)
        => alg.Verify(jwt.BytesToSign, jwt.Signature) ? null : JwtErrors.ErrorInvalidSign;

    public static string? CheckSign(JwtParts jwt, IJwtAlgorithm algorithm, byte[] key)
    {
        if (key.Length > 0)
        {
            var reSignature = algorithm.Sign(key, jwt.BytesToSign);
            return !reSignature.SequenceEqual(jwt.Signature)
                ? JwtErrors.ErrorInvalidSign
                : null;
        }
        else
        {
            return JwtErrors.ErrorArgumentIsInvalid(nameof(TokenParameters.SigningKey));
        }
    }

    #endregion

    private readonly TokenParameters _valParams;

    /// <summary>
    /// Creates an instance of <see cref="JwtValidator" /> with time margin
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer</param>
    /// <param name="dateTimeProvider">The DateTime provider</param>
    /// <param name="valParams">Validation parameters that are passed on to <see cref="JwtValidator"/></param>
    /// <param name="urlEncoder">The base64 URL Encoder</param>
    public JwtValidator(TokenParameters valParams)
    {
        _valParams = Throw.IsArgumentNullException(valParams, nameof(valParams));
        if (!valParams.Validate(out var message))
        {
            throw new ArgumentNullException(message);
        }
    }


    /// <inheritdoc />
    /// <exception cref="ArgumentException" />
    public bool TryValidate(JwtParts jwt, out string? error)
    {
        error = Validate(jwt);
        return error is null;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException" />
    public string? Validate(JwtParts jwt)
    {
        Throw.IsArgumentNullException(jwt, nameof(jwt));

        var error = _valParams.ValidateSignature
            ? _valParams.Algorithm is IAsymmetricAlgorithm asymmAlg
                    ? CheckSign(jwt, asymmAlg)
                    : _valParams.SigningKey is null
                        ? JwtErrors.ErrorArgumentIsNull(nameof(_valParams.SigningKey))
                        : CheckSign(jwt, _valParams.Algorithm!, _valParams.SigningKey)
            : CheckNoneAlgorithm(jwt.EnsureHeader(_valParams.JsonSerializer));

        return error ?? CheckPayload(jwt);
    }


    private string? CheckPayload(JwtParts jwt)
    {
        var payload = jwt.EnsurePayload(_valParams.JsonSerializer);

        var error = _valParams.ValidateAudience
            ? CheckClaimAud(payload, _valParams.ValidAudience)
            : null;

        error ??= _valParams.ValidateIssuer
            ? CheckClaimIss(payload, _valParams.ValidIssuer)
            : null;

        if (_valParams.ValidateExpirationTime || _valParams.ValidateIssuedTime)
        {
            var now = _valParams.DateTimeProvider.GetNow();
            var secondsSinceEpoch = UnixEpoch.GetSecondsSince(now);

            error ??= _valParams.ValidateExpirationTime
                ? CheckClaimExp(payload, secondsSinceEpoch, _valParams.TimeMargin)
                : null;

            error ??= _valParams.ValidateIssuedTime
                ? CheckClaimNbf(payload, secondsSinceEpoch, _valParams.TimeMargin) ?? CheckClaimIat(payload, secondsSinceEpoch, _valParams.TimeMargin)
                : null;
        }

        return error;
    }
}