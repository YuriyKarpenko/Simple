using System.Security.Cryptography;

namespace Simple.Web.Jwt.Algorithms;

/// <summary>
/// HMAC using SHA-512
/// </summary>
public sealed class HmacSha512 : HmacSha
{
    /// <inheritdoc />
    public override string Name => nameof(JwtAlgorithmName.HS512);

    /// <inheritdoc />
    public override HashAlgorithmName HashAlgorithmName => HashAlgorithmName.SHA512;

    protected override HMAC CreateAlgorithm(byte[] key) => new HMACSHA512(key);
}