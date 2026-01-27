using System.Text;

using Simple.Helpers;
using Simple.Jwt.Algorithms;

namespace Tests.Jwt;
public class JwtEncoderTests : BaseTests
{
    private const string KeyStr = "key";
    private static readonly JWT.IJwtEncoder s_svc = new JWT.JwtEncoder(new JWT.Algorithms.HMACSHA256Algorithm(), new JWT.Serializers.JsonNetSerializer(), new JWT.JwtBase64UrlEncoder());
    public JwtEncoderTests()
    {
        Algorithm = new HmacSha256();
    }


    [Theory]
    [InlineData("1")]
    public void OptEncode(string field)
    {
        //  Arrange
        Payload.SetClaim("custom field", field);
        var key = Encoding.ASCII.GetBytes(KeyStr);
        var tokenExpected = s_svc.Encode(null, Payload, key);

        //  test
        var o = this.OptEncoder().Then(i => i.OptEncode(Payload, key));

        //  assert
        Assert.True(o.HasValue);
        Assert.Equal(tokenExpected, o.Value);
    }

    [Theory]
    [InlineData("1")]
    public void TryEncode(string field)
    {
        //  Arrange
        Payload.SetClaim("custom field", field);
        var key = Encoding.ASCII.GetBytes(KeyStr);
        var tokenExpected = s_svc.Encode(null, Payload, key);

        var actual = this.TryCreateEncoder(out var svc, out var error);


        //  test
        actual &= svc.TryEncode(Payload, key, out var tokenActual, out error);

        //  assert
        Assert.True(actual);
        Assert.Equal(tokenExpected, tokenActual);
    }
}