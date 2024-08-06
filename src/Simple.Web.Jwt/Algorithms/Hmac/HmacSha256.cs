using System.Security.Cryptography;

namespace Simple.Web.Jwt.Algorithms;

/// <summary>
/// HMAC using SHA-256
/// </summary>
public sealed class HmacSha256 : HmacSha
{
    /// <inheritdoc />
    public override string Name => nameof(JwtAlgorithmName.HS256);

    /// <inheritdoc />
    public override HashAlgorithmName HashAlgorithmName => HashAlgorithmName.SHA256;

    protected override HMAC CreateAlgorithm(byte[] key) => new HMACSHA256(key);
}