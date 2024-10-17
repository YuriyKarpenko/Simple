using Simple.Logging.Configuration;

namespace Test.Logging;
public class LogOptionsTests
{
    private readonly LogOptions svc = LogOptions.Instance;


    [Theory]
    [InlineData("qwerty")]
    [InlineData("asdfgh")]
    public void EnsureOptionItem(string observerName)
    {
        //  test
        svc.EnsureOptionItem(observerName);

        //  assert
        Assert.True(svc.ContainsKey(observerName));
    }

    [Theory]
    [InlineData("qwer", LogLevel.Critical)]
    [InlineData("asdf", LogLevel.Debug)]
    public void SetFilterItem(string observerName, LogLevel level)
    {
        //  arrangr
        var fi = new LoggerFilterItem(null, level);

        //  test
        svc.SetFilterItem(observerName, fi);

        //  assert
        Assert.True(svc.ContainsKey(observerName));
        Assert.Equal(fi, svc[observerName].FilterItem);
    }

    [Theory]
    [InlineData(LoggerFilterItemTests.NsRoot + ".Ns_0.Ns_1", LogLevel.None, true)]          //  current = Error     : svc.Default <= None
    [InlineData(LoggerFilterItemTests.NsRoot + ".Ns_0.Ns_1", LogLevel.Error, true)]         //  current = Error     : current <= Error + observer.Default = Error
    [InlineData(LoggerFilterItemTests.NsRoot + ".Ns_0.Ns_1.Ns_2", LogLevel.Error, true)]    //  current = Warning   : current <= Error + observer.Default = Error
    [InlineData(LoggerFilterItemTests.NsRoot + ".Ns_0.Ns_1.Ns_2.Ns_3", LogLevel.Error, true)]//  current = Info     : observer.Default = Error
    [InlineData(LoggerFilterItemTests.NsRoot + ".Ns_0.Ns_1", LogLevel.Warning, false)]      //  current = Error     : current > Warning
    [InlineData(LoggerFilterItemTests.NsRoot + ".Ns_0.Ns_1.Ns_2", LogLevel.Warning, true)]  //  current = Warning   : current <= Warning
    [InlineData(LoggerFilterItemTests.NsRoot + ".Ns_0.Ns_1.Ns_2", LogLevel.Info, false)]    //  current = Warning
    //[MemberData()]
    public void FilterIn(string loggerName, LogLevel level, bool expected)
    {
        //  arrangr
        LoggerFilterItemTests.TestRules.Remove(string.Empty);
        svc.SetFilterItem("some observer name", new LoggerFilterItem(LoggerFilterItemTests.TestRules));
        svc.Default.MinLevel = LogLevel.Critical;

        //  test
        var actual = svc.FilterIn(level, loggerName);

        //  assert
        Assert.Equal(expected, actual);
    }
}
