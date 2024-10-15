using System.Collections.Generic;
using System.IO;

using Simple.Configuration;
using Simple.Configuration.Sources;

namespace Test.Configuration.Sources;
public class JsonSourceTests
{
    [Theory]
    [InlineData("file.ext")]
    [InlineData("/Path1/path2/file.ext")]
    public void Build(string file)
    {
        const string basePath = "/base/path";
        //  arrange
        var props = new Dictionary<string, object>
        {
            { "BasePath", basePath },
        };
        var builder = Substitute.For<IConfigurationBuilder>();
        builder.Properties.Returns(props);
        var config = Substitute.For<IConfiguration>();
        var isFile = file == Path.GetFileName(file);
        var js = Substitute.ForPartsOf<JsonSource>();
        //  disable exeption
        js.IsOptional = true;
        if (isFile)
        {
            js.FileName = file;
        }
        else
        {
            js.FullPath = file;
        }

        //  test
        js.Build(builder, config);

        //  assert
        Assert.Equal(file, isFile ? js?.FileName : js?.FullPath);
        if (isFile)
        {
            Assert.Equal(Path.Combine(basePath, file), js?.FullPath);
        }
    }
}
