using System;

using Simple.Helpers;
using Simple.Web.Jwt;

namespace Tests.Jwt;
public class JwtValidatorTests
{
    #region Claimes

    static readonly TimeSpan _timeMargin = TimeSpan.FromSeconds(5);

    [Theory]
    [InlineData("val", "val", true)]
    [InlineData("val", "vAl", false)]
    public void CheckClaimAud(string actualValue, string expectedValue, bool expected)
    {
        //  arrange
        var pl = new Payload().Audience(actualValue);

        //  test
        var err = JwtValidator.CheckClaimAud(pl, expectedValue);

        //  assert
        Assert.Equal(expected, string.IsNullOrEmpty(err));
    }

    [Theory]
    [InlineData("val", "val", true)]
    [InlineData("val", "vAl", false)]
    public void CheckClaimIss(string actualValue, string expectedValue, bool expected)
    {
        //  arrange
        var pl = new Payload().Issuer(actualValue);

        //  test
        var err = JwtValidator.CheckClaimIss(pl, expectedValue);

        //  assert
        Assert.Equal(expected, string.IsNullOrEmpty(err));
    }

    [Theory]
    [InlineData(10, 14, true)]
    [InlineData(10, 16, false)]
    public void CheckClaimExp(long actualValue, long expectedValue, bool expected)
    {
        //  arrange
        var pl = new Payload().ExpirationTime(actualValue);

        //  test
        var err = JwtValidator.CheckClaimExp(pl, expectedValue, _timeMargin);

        //  assert
        Assert.Equal(expected, string.IsNullOrEmpty(err));
    }

    [Theory]
    [InlineData(10, 4, false)]
    [InlineData(10, 6, true)]
    public void CheckClaimIat(long actualValue, long expectedValue, bool expected)
    {
        //  arrange
        var pl = new Payload().IssuedAt(actualValue);

        //  test
        var err = JwtValidator.CheckClaimIat(pl, expectedValue, _timeMargin);

        //  assert
        Assert.Equal(expected, string.IsNullOrEmpty(err));
    }

    [Theory]
    [InlineData(10, 4, false)]
    [InlineData(10, 6, true)]
    public void CheckClaimNbf(long actualValue, long expectedValue, bool expected)
    {
        //  arrange
        var pl = new Payload().NotBefore(actualValue);

        //  test
        var err = JwtValidator.CheckClaimNbf(pl, expectedValue, _timeMargin);

        //  assert
        Assert.Equal(expected, string.IsNullOrEmpty(err));
    }

    #endregion


    private class Payload : DicString<object>, IJwtPayload
    {
    }
}
