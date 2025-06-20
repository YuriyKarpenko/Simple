using Newtonsoft.Json.Linq;

using Simple.Logging.Configuration;

namespace Test.Logging;
public class LogOptionItemRawTests
{
    private readonly LogOptionItemRaw svc = new LogOptionItemRaw(LogLevel.Critical);


    [Theory]
    [InlineData(@"{
        ""LogLevel"": {
            ""Microsoft.Extensions.Hosting"": ""Information"",
            ""Simple.DI"": ""Debug"", 
            ""Default"": ""Warning"" 
        },
        ""IncludeScope"": ""false""
    }", 1, LogLevel.Warning, 2)]
    public void Json(string json, int expectedCount, LogLevel expectedDefault, int expectedRulesCount)
    {
        //  arrange
        var jo = JObject.Parse(json);

        //  test
        svc.Populate(jo);

        //  assert
        Assert.Equal(expectedCount, svc.Count);
        Assert.Equal(expectedDefault, svc.LogLevel.Default);
        Assert.Equal(expectedRulesCount, svc.LogLevel.Count);
    }
}