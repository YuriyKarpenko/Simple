using System;

namespace Test.Helpers;

public class ValidationTests
{

    #region InRange

    [Theory]
    [InlineData(5, 1, 10, true)]
    [InlineData(1, 1, 10, true)]
    [InlineData(10, 1, 10, true)]
    [InlineData(0, 1, 10, false)]
    [InlineData(11, 1, 10, false)]
    [InlineData("b", "a", "c", true)]
    [InlineData("z", "a", "c", false)]
    [InlineData(null, "a", "z", false)]
    public void InRange<T>(T value, T min, T max, bool expected) where T : IComparable
    {
        Assert.Equal(expected, value.InRange(min, max));
    }

    #endregion

    #region Email

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("a.b-c@sub.domain.org", true)]
    [InlineData("plainaddress", false)]
    [InlineData("@missinguser.com", false)]
    [InlineData("user@nodot", false)]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("١٢٣@example.com", false)]      //  Arabic-Indic digits
    [InlineData("test@exämple.com", false)]     //  Unicode in domain
    [InlineData("test@例え.com", false)]        //  CJK in domain
    public void IsEmail(string? value, bool expected)
    {
        Assert.Equal(expected, value.IsEmail());
    }

    #endregion

    #region Ip

    [Theory]
    [InlineData(null, false)]
    [InlineData("2", false)]
    [InlineData("2.2", false)]
    [InlineData("2.2.2", false)]
    [InlineData("22.22.22,22", false)]
    [InlineData("22.22.22-22", false)]
    [InlineData("22.22.22.22.", false)]
    [InlineData("22..22.22.22", false)]
    [InlineData(".22.22.22.22", false)]
    [InlineData("000.22.22.22", false)]
    [InlineData("00.22.22.22", false)]
    [InlineData("0.22.22.22", false)]
    [InlineData("01.22.22.22", false)]
    [InlineData("1.22.22.0", false)]
    [InlineData("1.22.22.01", false)]
    [InlineData("22.0.0.22", true)]
    [InlineData("106.222.168.74", true)]
    [InlineData("27.61.77.120", true)]
    [InlineData("22.0.00.22", false)]
    [InlineData("90.202.202.220", true)]
    [InlineData("90.202.202.022", false)]
    [InlineData("99.22.22.22", true)]
    [InlineData("249.22.22.22", true)]
    [InlineData("259.22.22.22", false)]
    [InlineData("255.22.22.22", true)]
    [InlineData("256.22.22.22", false)]
    [InlineData("099.22.22.22", false)]
    public void Ip(string? ip, bool expected)
    {
        Assert.Equal(expected, ExtensionsValidation.IsIp(ip));
    }

    #endregion

}