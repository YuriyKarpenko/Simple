namespace Tests.Jwt;
public class JwtPartsTests
{
    private static readonly IBase64UrlEncoder _base64UrlEncoder = new Base64UrlEncoder();

    [Theory]
    //[InlineData(null, false)]   //  exception
    [InlineData(".e30", false)]         //  ".{}"
    [InlineData(".e30.", false)]        //  ".{}."
    [InlineData("e30..", false)]        //  "{}.."
    [InlineData("e30.e30.", true)]      //  "{}.{}."
    [InlineData("e30.e30", false)]      //  "{}.{}"
    [InlineData("e30.e30.cXdl", true)]  //  "{}.{}.qwe"
    [InlineData("e30.e30.poh", true)]   //  "{}.{}.???"
    public void OptParse(string token, bool expected)
    {
        //  test
        var o = JwtParts.OptParse(token, _base64UrlEncoder);
        //_base64UrlEncoder.TryEncode("qwe".GetBytes(), out var token64, out error);

        //  assert
        Assert.Equal(expected, o.HasValue);
    }

    [Theory]
    //[InlineData(null, false)]     //  exception
    [InlineData(".e30", false)]         //  ".{}"
    [InlineData(".e30.", false)]        //  ".{}."
    [InlineData("e30..", false)]        //  "{}.."
    [InlineData("e30.e30.", true)]      //  "{}.{}."
    [InlineData("e30.e30", false)]      //  "{}.{}"
    [InlineData("e30.e30.cXdl", true)]  //  "{}.{}.qwe"
    [InlineData("e30.e30.poh", true)]   //  "{}.{}.???"
    public void TryParse(string token, bool expected)
    {
        //  test
        var actual = JwtParts.TryParse(token, _base64UrlEncoder, out var jwt, out var error);
        //_base64UrlEncoder.TryEncode("qwe".GetBytes(), out var token64, out error);

        //  assert
        Assert.Equal(expected, actual);
        Assert.Equal(expected, error is null);
    }
}