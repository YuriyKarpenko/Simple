//global using IJwtPayload = System.Collections.Generic.IDictionary<string, object>;

using System.Globalization;

using System.Linq;
namespace Simple.Jwt;

public interface IJwtPayload : IDictionary<string, object> { }

internal class JwtPayload : DicString<object>, IJwtPayload
{
    public JwtPayload() : base(StringComparer.OrdinalIgnoreCase) { }
    public JwtPayload(IJwtPayload external) : base(external, StringComparer.OrdinalIgnoreCase) { }
}

public static class JwtBuilderExtensions
{
    /// <summary> Set claim value using <see cref="ClaimName"/> or custom. </summary>
    /// <param name="builder">IJwtPayload instance</param>
    /// <param name="claimName">Claim name</param>
    /// <param name="value">Claim value</param>
    public static IJwtPayload SetClaim(this IJwtPayload builder, string claimName, object value)
    {
        builder[claimName] = value;
        return builder;
    }

    /// <summary> Adds several claims to the JWT </summary>
    public static IJwtPayload AddClaims(this IJwtPayload builder, IEnumerable<KeyValuePair<string, object>> claims)
        => claims.Aggregate(builder, (b, p) => b.SetClaim(p.Key, p.Value));

    public static IJwtPayload ExpirationTime(this IJwtPayload builder, DateTime time)
        => builder.SetClaim(ClaimName.ExpirationTime, UnixEpoch.GetSecondsSince(time));
    public static IJwtPayload ExpirationTime(this IJwtPayload builder, long time)
        => builder.SetClaim(ClaimName.ExpirationTime, time);

    public static IJwtPayload Issuer(this IJwtPayload builder, string issuer)
        => builder.SetClaim(ClaimName.Issuer, issuer);

    public static IJwtPayload Subject(this IJwtPayload builder, string subject)
        => builder.SetClaim(ClaimName.Subject, subject);

    public static IJwtPayload Audience(this IJwtPayload builder, string audience)
        => builder.SetClaim(ClaimName.Audience, audience);

    public static IJwtPayload NotBefore(this IJwtPayload builder, DateTime time)
        => builder.SetClaim(ClaimName.NotBefore, UnixEpoch.GetSecondsSince(time));
    public static IJwtPayload NotBefore(this IJwtPayload builder, long time)
        => builder.SetClaim(ClaimName.NotBefore, time);

    public static IJwtPayload IssuedAt(this IJwtPayload builder, DateTime time)
        => builder.SetClaim(ClaimName.IssuedAt, UnixEpoch.GetSecondsSince(time));
    public static IJwtPayload IssuedAt(this IJwtPayload builder, long time)
        => builder.SetClaim(ClaimName.IssuedAt, time);

    public static IJwtPayload Id(this IJwtPayload builder, Guid id)
        => builder.Id(id.ToString());
    public static IJwtPayload Id(this IJwtPayload builder, long id)
        => builder.Id(id.ToString(CultureInfo.InvariantCulture));
    public static IJwtPayload Id(this IJwtPayload builder, string id)
        => builder.SetClaim(ClaimName.JwtId, id);

    public static IJwtPayload GivenName(this IJwtPayload builder, string name)
        => builder.SetClaim(ClaimName.GivenName, name);

    public static IJwtPayload FamilyName(this IJwtPayload builder, string lastname)
        => builder.SetClaim(ClaimName.FamilyName, lastname);

    public static IJwtPayload MiddleName(this IJwtPayload builder, string middleName)
        => builder.SetClaim(ClaimName.MiddleName, middleName);
}