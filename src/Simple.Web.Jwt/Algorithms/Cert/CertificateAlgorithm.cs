﻿using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Simple.Web.Jwt.Algorithms;

public abstract class CertificateAlgorithm<T> : IAsymmetricAlgorithm
    where T : class
{
    protected readonly T _publicKey;
    protected readonly T _privateKey;

    protected CertificateAlgorithm(T publicKey, T privateKey)
    {
        _publicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        _privateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
    }

    protected CertificateAlgorithm(T publicKey)
    {
        _publicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        _privateKey = null;
    }

    protected CertificateAlgorithm(X509Certificate2 cert)
    {
        if (cert is null)
        {
            throw new InvalidOperationException(nameof(cert));
        }

        _publicKey = GetPublicKey(cert) ?? throw new Exception("Certificate must have public key.");
        _privateKey = GetPrivateKey(cert);
    }

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public abstract HashAlgorithmName HashAlgorithmName { get; }

    /// <inheritdoc />
    public byte[] Sign(byte[] key, byte[] bytesToSign) =>
        Sign(bytesToSign);

    /// <summary>
    /// Signs the provided byte array with the private key.
    /// </summary>
    public byte[] Sign(byte[] bytesToSign)
    {
        return bytesToSign is null
            ? throw new ArgumentNullException(nameof(bytesToSign))
            : _privateKey is null
                ? throw new InvalidOperationException("Can't sign data without private key")
                : SignData(bytesToSign);
    }

    /// <inheritdoc />
    public bool Verify(byte[] bytesToSign, byte[] signature)
    {
        return bytesToSign is null
            ? throw new ArgumentNullException(nameof(bytesToSign))
            : signature is null
                ? throw new ArgumentNullException(nameof(signature))
                : VerifyData(bytesToSign, signature);
    }

    protected abstract T GetPublicKey(X509Certificate2 cert);

    protected abstract T GetPrivateKey(X509Certificate2 cert);

    protected abstract byte[] SignData(byte[] bytesToSign);

    protected abstract bool VerifyData(byte[] bytesToSign, byte[] signature);
}