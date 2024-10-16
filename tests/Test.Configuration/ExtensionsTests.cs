using System.Collections.Generic;
using System.IO;

using Simple.Configuration;
using Simple.Configuration.Sources;

namespace Test.Configuration;

public class ExtensionsTests
{
    //protected readonly 
    public ExtensionsTests()
    {
        //Newtonsoft.Json.
    }

    [Theory]
    [InlineData("file.ext")]
    [InlineData("/Path1/path2/file.ext")]
    public void AddJsonFile(string file)
    {
        //  arrange
        var isFile = file == Path.GetFileName(file);
        JsonSource? js = null;
        var sources = Substitute.For<IList<IConfigurationSource>>();
        var builder = Substitute.For<IConfigurationBuilder>();
        builder.Sources.Returns(sources);

        sources.When(l => l.Add(Arg.Any<JsonSource>())).Do(ci => js = ci[0] as JsonSource);

        //  test
        builder.AddJsonFile(file);

        //  assert
        sources.Received(1).Add(Arg.Any<JsonSource>());
        Assert.Equal(file, isFile ? js?.FileName : js?.FullPath);
    }
}