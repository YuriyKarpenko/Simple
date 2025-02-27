using Newtonsoft.Json.Linq;

using Simple.Helpers;
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
        var fi = new LoggerFilterItem(level);

        //  test
        svc.SetFilterItem(observerName, fi);

        //  assert
        Assert.True(svc.ContainsKey(observerName));
        Assert.Equal(fi, svc[observerName].LogLevel);
    }

    [Theory]
    [InlineData(LoggerFilterItemTests.NsRoot + ".Ns_0.Ns_1", LogLevel.None, true)]          //  current = Error     : svc.Default <= None
    [InlineData(LoggerFilterItemTests.NsRoot + ".Ns_0.Ns_1", LogLevel.Error, true)]         //  current = Error     : current <= Error + observer.Default = Error
    [InlineData(LoggerFilterItemTests.NsRoot + ".Ns_0.Ns_1.Ns_2", LogLevel.Error, true)]    //  current = Warning   : current <= Error + observer.Default = Error
    [InlineData(LoggerFilterItemTests.NsRoot + ".Ns_0.Ns_1.Ns_2.Ns_3", LogLevel.Error, true)]//  current = Info     : observer.Default = Error
    [InlineData(LoggerFilterItemTests.NsRoot + ".Ns_0.Ns_1", LogLevel.Warning, false)]      //  current = Error     : current > Warning
    [InlineData(LoggerFilterItemTests.NsRoot + ".Ns_0.Ns_1.Ns_2", LogLevel.Warning, true)]  //  current = Warning   : current <= Warning
    [InlineData(LoggerFilterItemTests.NsRoot + ".Ns_0.Ns_1.Ns_2", LogLevel.Information, false)]    //  current = Warning
    //[MemberData()]
    public void FilterIn(string loggerName, LogLevel level, bool expected)
    {
        //  arrangr
        LoggerFilterItemTests.TestRules.Remove(string.Empty);
        var fi = new LoggerFilterItem();
        fi.Merge(LoggerFilterItemTests.TestRules);
        svc.SetFilterItem("some observer name", fi);
        svc.LogLevel.Default = LogLevel.Critical;

        //  test
        var actual = svc.FilterIn(level, loggerName);

        //  assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(@"{
        ""Debug"": {
            ""LogLevel"": {
                ""Microsoft.Extensions.Hosting"": ""Info"",
                ""Simple.DI"": ""Debug"", 
                ""Default"": ""Warning"" 
            },
        },
        ""LogLevel"": {
            ""Microsoft.Extensions.Hosting"": ""Info"",
            ""Simple.DI"": ""Trace"", 
            ""Default"": ""Trace"" 
        },
        ""Console"": {
            ""LogLevel"": {
                ""Microsoft.Extensions.Hosting"": ""Info"",
                ""Simple.DI"": ""Debug"", 
                ""Default"": ""Warning"" 
            },
            ""IncludeScope"": ""false""
        }
    }", 2, LogLevel.Trace)]
    public void Json(string json, int expectedCount, LogLevel expectedDefault)
    {
        //  arrange
        svc.Clear();
        var jo = JObject.Parse(json);

        //  test
        svc.Populate(jo);

        //  assert
        Assert.Equal(expectedDefault, svc.LogLevel.Default);
        Assert.Equal(expectedCount, svc.Count);
    }
}