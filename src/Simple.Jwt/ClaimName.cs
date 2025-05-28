namespace Simple.Jwt;

/// <summary>
/// All public claims of a JWT specified by IANA = ""; see https://www.iana.org/assignments/jwt/jwt.xhtml
/// </summary>
public static class ClaimName
{
    //  https://datatracker.ietf.org/doc/html/rfc7519#section-10.1.2
    public const string Issuer = "iss";

    public const string Subject = "sub";

    public const string Audience = "aud";

    public const string ExpirationTime = "exp";

    public const string NotBefore = "nbf";

    public const string IssuedAt = "iat";

    public const string JwtId = "jti";

    //  other names
    public const string FullName = "name";

    public const string GivenName = "given_name";

    public const string FamilyName = "family_name";

    public const string MiddleName = "middle_name";

    public const string CasualName = "nickname";

    public const string PreferredUsername = "preferred_username";

    public const string ProfilePageUrl = "profile";

    public const string ProfilePictureUrl = "picture";

    public const string Website = "website";

    public const string PreferredEmail = "email";

    public const string VerifiedEmail = "email_verified";

    public const string Gender = "gender";

    public const string Birthday = "birthdate";

    public const string TimeZone = "zoneinfo";

    public const string Locale = "locale";

    public const string PreferredPhoneNumber = "phone_number";

    public const string VerifiedPhoneNumber = "phone_number_verified";

    public const string Address = "address";

    public const string UpdatedAt = "update_at";

    public const string AuthorizedParty = "azp";

    public const string Nonce = "nonce";

    public const string AuthenticationTime = "auth_time";

    public const string AccessTokenHash = "at_hash";

    public const string CodeHashValue = "c_hash";

    public const string Acr = "acr";

    public const string Amr = "amr";

    public const string PublicKey = "sub_jwk";

    public const string Confirmation = "cnf";

    public const string SipFromTag = "sip_from_tag";

    public const string SipDate = "sip_date";

    public const string SipCallId = "sip_callid";

    public const string SipCseqNumber = "sip_cseq_num";

    public const string SipViaBranch = "sip_via_branch";

    public const string OriginatingIdentityString = "orig";

    public const string DestinationIdentityString = "dest";

    public const string MediaKeyFingerprintString = "mky";
}