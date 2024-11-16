using System.Collections.Generic;
using System.Linq;

using Simple.Helpers;
using Simple.Logging.Configuration;

namespace Test.Logging
{
    public class LoggerFilterItemTests
    {
        public const string NsRoot = "root namespace";
        private static readonly string[] Ns = Enumerable.Range(0, 8).Select(i => $"Ns_{i}").ToArray();
        public static readonly Dictionary<string, LogLevel> TestRules = new Dictionary<string, LogLevel>
        {
            { $"{NsRoot}.{Ns[0]}.{Ns[1]}.{Ns[2]}.{Ns[3]}.{Ns[4]}.{Ns[5]}", LogLevel.Trace },
            { $"{NsRoot}.{Ns[0]}.{Ns[1]}.{Ns[2]}.{Ns[3]}.{Ns[4]}", LogLevel.Debug },
            { $"{NsRoot}.{Ns[0]}.{Ns[1]}.{Ns[2]}.{Ns[3]}", LogLevel.Info },
            { $"{NsRoot}.{Ns[0]}.{Ns[1]}.{Ns[2]}", LogLevel.Warning },
            { $"{NsRoot}.{Ns[0]}.{Ns[1]}", LogLevel.Error },
            { $"{NsRoot}.{Ns[0]}", LogLevel.Critical },
            { $"{NsRoot}", LogLevel.None },
            { string.Empty, LogLevel.Warning },     //  custom default
        };


        private readonly LoggerFilterItem svc = new LoggerFilterItem();


        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Info)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.None)]
        [InlineData(LogLevel.Error, true)]  //  use default
        public void MinLevel(LogLevel expected, bool remove = false)
        {
            //  arrange
            var rules = new Dictionary<string, LogLevel>(TestRules);
            if (remove)
            {
                rules.Remove(string.Empty);
            }
            else
            {
                rules[string.Empty] = expected;
            }

            //  test
            svc.Merge(rules);

            //  assert
            Assert.Equal(expected, svc.Default);
        }


        [Theory]
        [InlineData(NsRoot + ".Ns_0.Ns_1.Ns_2.Ns_3.Ns_4.Ns_5.ClassName", LogLevel.Trace, true)]
        [InlineData(NsRoot + ".Ns_0.Ns_1.Ns_2.Ns_3.Ns_4.Ns_5.ClassName", LogLevel.Debug, true)]
        [InlineData(NsRoot + ".Ns_0.Ns_1.Ns_2.Ns_3.Ns_4.ClassName", LogLevel.Trace, false)]
        [InlineData(NsRoot + ".Ns_0.Ns_1.Ns_2.Ns_3.Ns_4.ClassName", LogLevel.Debug, true)]
        [InlineData(NsRoot + ".Ns_0.Ns_1.Ns_2.Ns_3.ClassName", LogLevel.Trace, false)]
        [InlineData(NsRoot + ".Ns_0.Ns_1.Ns_2.Ns_3.ClassName", LogLevel.Debug, false)]
        [InlineData(NsRoot + ".Ns_0.Ns_1.Ns_2.Ns_3.ClassName", LogLevel.Info, true)]
        //  Allows by .Level >= MinLevel
        [InlineData(NsRoot + ".Ns_0.Ns_1.Ns_2.ClassName", LogLevel.Trace, false)]
        [InlineData(NsRoot + ".Ns_0.Ns_1.Ns_2.ClassName", LogLevel.Debug, false)]
        [InlineData(NsRoot + ".Ns_0.Ns_1.Ns_2.ClassName", LogLevel.Info, false)]
        public void Filter(string fullName, LogLevel level, bool expected)
        {
            //  arrange
            svc.Merge(TestRules);

            //  test
            var actual = svc.Filter(level, fullName);

            //  assert
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("{ \"Microsoft.Extensions.Hosting\": \"Warning\", \"Default\": \"Info\" }", 1, LogLevel.Info)]
        [InlineData("{ \"Simple.Logging\": \"Info\", \"Simple.DI\": \"Info\", \"Default\": \"Warning\" }", 2, LogLevel.Warning)]
        public void Json(string json, int expectedCount, LogLevel expectedDefault)
        {
            //  test
            Newtonsoft.Json.JsonConvert.PopulateObject(json, svc);

            //  assert
            Assert.Equal(expectedDefault, svc.Default);
            Assert.Equal(expectedCount, svc.Count);
        }
    }
}