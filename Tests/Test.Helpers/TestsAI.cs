using System;
using System.Text;
using System.Text.Json;

namespace Test.Helpers;

public class ConverterTests
{
    [Fact]
    public void SerializeDeserialize()
    {
        var opt = new JsonSerializerOptions();
        opt.Converters.Add(new JsonConverterBytesHex());

        var data = new byte[] { 10, 20, 30 };

        var json = JsonSerializer.Serialize(data, opt);
        var back = JsonSerializer.Deserialize<byte[]>(json, opt);

        Assert.Equal(data, back);
    }

    [Fact]
    public void NullHandling()
    {
        var opt = new JsonSerializerOptions();
        opt.Converters.Add(new JsonConverterBytesHex());

        byte[]? data = null;

        var json = JsonSerializer.Serialize(data, opt);
        var back = JsonSerializer.Deserialize<byte[]>(json, opt);

        Assert.Null(back);
    }
}