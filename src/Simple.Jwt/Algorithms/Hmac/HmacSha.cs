using System.Security.Cryptography;

namespace Simple.Jwt.Algorithms;

public abstract class HmacSha : IJwtAlgorithm
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract HashAlgorithmName HashAlgorithmName { get; }

    /// <inheritdoc />
    public byte[] Sign(byte[] key, byte[] bytesToSign)
    {
        using var sha = CreateAlgorithm(key);
        return sha.ComputeHash(bytesToSign);
    }

    protected abstract HMAC CreateAlgorithm(byte[] key);
}