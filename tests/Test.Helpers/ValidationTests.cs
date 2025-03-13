namespace Test.Helpers;

public class ValidationTests
{
    #region Ip

    [Theory]
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
    public void Ip(string ip, bool expected)
    {
        //  action
        var actual = ExtensionsValidation.IsIp(ip);

        //  assert
        Assert.Equal(expected, actual);
    }

    #endregion

}
