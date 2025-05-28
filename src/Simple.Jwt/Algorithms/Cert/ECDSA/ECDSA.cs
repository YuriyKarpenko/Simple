using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Simple.Jwt.Algorithms;

/// <summary>
/// Abstract base class for all ECDSA algorithms
/// </summary>
public abstract class ECDSA : CertificateAlgorithm<ECDsa>
{
    /// <summary>
    /// Creates an instance of <see cref="ECDSA" /> using the provided pair of public and private keys.
    /// </summary>
    /// <param name="publicKey">The public key for verifying the data.</param>
    /// <param name="privateKey">The private key for signing the data.</param>
    protected ECDSA(ECDsa publicKey, ECDsa privateKey)
        : base(publicKey, privateKey)
    {
    }

    /// <summary>
    /// Creates an instance of <see cref="ECDSA" /> using the provided public key only.
    /// </summary>
    /// <remarks>
    /// An instance created using this constructor can only be used for verifying the data, not for signing it.
    /// </remarks>
    /// <param name="publicKey">The public key for verifying the data.</param>
    protected ECDSA(ECDsa publicKey)
        : base(publicKey)
    {
    }

    /// <summary>
    /// Creates an instance using the provided certificate.
    /// </summary>
    /// <param name="cert">The certificate having a public key and an optional private key.</param>
    protected ECDSA(X509Certificate2 cert)
        : base(cert)
    {
    }

    protected override ECDsa GetPublicKey(X509Certificate2 cert) =>
        cert.GetECDsaPublicKey();

    protected override ECDsa GetPrivateKey(X509Certificate2 cert) =>
        cert.GetECDsaPrivateKey();


    protected override byte[] SignData(byte[] bytesToSign) =>
        _privateKey.SignData(bytesToSign, HashAlgorithmName);

    protected override bool VerifyData(byte[] bytesToSign, byte[] signature) =>
        _publicKey.VerifyData(bytesToSign, signature, HashAlgorithmName);
}