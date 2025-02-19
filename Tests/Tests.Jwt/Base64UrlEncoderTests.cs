using Simple.Web.Jwt;

namespace Tests.Jwt;

public class Base64UrlEncoderTests : BaseTests
{
    private static readonly JWT.IBase64UrlEncoder s_svc = new JWT.JwtBase64UrlEncoder();
    private readonly IBase64UrlEncoder svc = new Base64UrlEncoder();

    [Theory]
    [InlineData("1", true)]     //  1 '='
    [InlineData("11", true)]    //  0 '='
    [InlineData("222", true)]   //  2 '='
    public void TryDecode(string alg, bool expected)
    {
        //  arrange
        _header.alg = alg;
        var input = s_svc.Encode(JsonSerializer.Serialize(_header).GetBytes());

        var outExpected = s_svc.Decode(input);

        //  test
        var actual = svc.TryDecode(input, out var outActual, out var error);

        //  assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected, string.IsNullOrEmpty(error));
        Assert.Equal(outExpected, outActual);
    }

    [Theory]
    [InlineData("1", true)]     //  0 '='
    [InlineData("11", true)]    //  1 '='
    [InlineData("222", true)]   //  2 '='
    [InlineData("1111", true)]  //  2 '='
    public void TryEncode(string alg, bool expected)
    {
        //  arrange
        _header.alg = alg;
        var input = JsonSerializer.Serialize(_header).GetBytes();

        var outExpected = s_svc.Encode(input);

        //  test
        var actual = svc.TryEncode(input, out var outActual, out var error);

        //  assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected, string.IsNullOrEmpty(error));
        Assert.Equal(outExpected, outActual);
    }
}