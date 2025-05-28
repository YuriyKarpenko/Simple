using System.Security.Cryptography;

namespace Simple.Jwt.Algorithms;

/// <summary>
/// HMAC using SHA-384
/// </summary>
public sealed class HmacSha384 : HmacSha
{
    /// <inheritdoc />
    public override string Name => nameof(JwtAlgorithmName.HS384);

    /// <inheritdoc />
    public override HashAlgorithmName HashAlgorithmName => HashAlgorithmName.SHA384;

    protected override HMAC CreateAlgorithm(byte[] key) => new HMACSHA384(key);
}