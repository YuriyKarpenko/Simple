using Simple.Web.Jwt.Algorithms;

namespace Simple.Web.Jwt;

/// <summary>
/// Represents a JWT encoder.
/// </summary>
public interface IJwtEncoder
{
    /// <summary>
    /// Creates a JWT given a header, a payload, the signing key, and the algorithm to use.
    /// </summary>
    /// <param name="payload">An arbitrary payload (must be serializable to JSON)</param>
    /// <param name="key">The key bytes used to sign the token</param>
    /// <param name="extraHeaders">An arbitrary set of extra headers. Will be augmented with the standard "typ" and "alg" headers</param>
    /// <returns>The generated JWT</returns>
    bool TryEncode(IJwtPayload payload, byte[]? key, out string output, out string? error, JwtHeader? extraHeaders = null);
}


/// <inheritdoc />
public sealed class JwtEncoder : IJwtEncoder
{
    private const string DotFormat = "{0}.{1}";
    private readonly IJwtAlgorithm _algorithm;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IBase64UrlEncoder _urlEncoder;

    /// <summary>
    /// Creates an instance of <see cref="JwtEncoder" />
    /// </summary>
    /// <param name="algorithm">The JWT algorithm</param>
    /// <param name="jsonSerializer">The JSON serializer</param>
    /// <param name="urlEncoder">The base64 URL encoder</param>
    public JwtEncoder(IJwtAlgorithm algorithm, IJsonSerializer jsonSerializer, IBase64UrlEncoder urlEncoder)
    {
        _algorithm = Throw.IsArgumentNullException(algorithm, nameof(algorithm));
        _jsonSerializer = Throw.IsArgumentNullException(jsonSerializer, nameof(jsonSerializer));
        _urlEncoder = Throw.IsArgumentNullException(urlEncoder, nameof(urlEncoder));
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException" />
    public bool TryEncode(IJwtPayload payload, byte[]? key, out string output, out string? error, JwtHeader? extraHeaders)
    {
        output = null!;

        if (!_algorithm.IsAsymmetric() && _algorithm is not NoneAlgorithm && !JwtErrors.IsArgumentNotNull(key, nameof(key), out error))
        {
            return false;
        }

        if (JwtErrors.IsArgumentNotNull(payload, nameof(payload), out error))
        {
            var header = EnsureHeader(_algorithm.Name, extraHeaders);

            if (TryEncode(header, out var headerSegment, out error) && TryEncode(payload, out var payloadSegment, out error))
            {
                var stringToSign = string.Format(DotFormat, headerSegment, payloadSegment);

                if (TryGetSignatureSegment(_algorithm, key, stringToSign, out var signatureSegment, out error))
                {
                    output = string.Format(DotFormat, stringToSign, signatureSegment);
                    return true;
                }
            }
        }

        return false;
    }

    private static JwtHeader EnsureHeader(string alg, JwtHeader? extraHeaders)
    {
        var header = extraHeaders ?? new JwtHeader();
        header.alg = alg;
        header.typ ??= JwtHeader.JwtType;
        return header;
    }

    private bool TryEncode(object value, out string output, out string? error)
    {
        var json = _jsonSerializer.Serialize(value);
        return _urlEncoder.TryEncode(json.GetBytes(), out output, out error);
    }

    private bool TryGetSignatureSegment(IJwtAlgorithm algorithm, byte[]? key, string stringToSign, out string output, out string? error)
    {
        error = null;
        output = string.Empty;
        if (algorithm is NoneAlgorithm)
        {
            return true;
        }

        var signature = algorithm.Sign(key!, stringToSign.GetBytes());
        return _urlEncoder.TryEncode(signature, out output, out error);
    }
}