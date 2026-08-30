using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.Helpers;

public class ExtensionsStreamTests
{
    [Fact]
    public async Task ReadToEndAsync_ReadsContent()
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes("hello world"));

        var result = await ms.ReadToEndAsync();

        Assert.Equal("hello world", result);
    }

    [Fact]
    public async Task ReadToEndAsync_SeeksToBeginning()
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes("hello world"));
        ms.Position = ms.Length;

        var result = await ms.ReadToEndAsync();

        Assert.Equal("hello world", result);
    }

    [Fact]
    public async Task ReadToEndAsync_NullStream_Throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((Stream)null!).ReadToEndAsync()!);
    }

    [Fact]
    public async Task ReadToEndAsync_NonReadable_ReturnsNull()
    {
        using var ms = new MemoryStream();
        ms.SetLength(0);
        var nonReadable = new WriteOnlyStream();

        var result = await nonReadable.ReadToEndAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task WriteAsync_Bytes_WritesAll()
    {
        using var ms   = new MemoryStream();
        var       data = Encoding.UTF8.GetBytes("abc");

        await ms.WriteAsync(data);

        Assert.Equal("abc", Encoding.UTF8.GetString(ms.ToArray()));
    }

    [Fact]
    public async Task WriteAsync_String_WithEncoding_ReturnsLength()
    {
        using var ms = new MemoryStream();

        var written = await ms.WriteAsync("hello", Encoding.UTF8);

        Assert.Equal(5, written);
        Assert.Equal("hello", Encoding.UTF8.GetString(ms.ToArray()));
    }

    [Fact]
    public async Task WriteAsync_String_DefaultsToUtf8()
    {
        using var ms = new MemoryStream();

        var written = await ms.WriteAsync("hello");

        Assert.Equal(5, written);
        Assert.Equal("hello", Encoding.UTF8.GetString(ms.ToArray()));
    }

    [Fact]
    public async Task WriteAsync_EmptyString_ReturnsZero()
    {
        using var ms = new MemoryStream();

        var written = await ms.WriteAsync("", Encoding.UTF8);

        Assert.Equal(0, written);
        Assert.Equal(0, ms.Length);
    }

    [Fact]
    public async Task WriteAsync_NullStream_Throws_EvenForEmptyContent()
    {
        //  guards run before the empty-content early return
        await Assert.ThrowsAsync<ArgumentNullException>(() => ((Stream)null!).WriteAsync("", Encoding.UTF8));
    }

    [Fact]
    public async Task WriteAsync_NullEncoding_Throws_EvenForEmptyContent()
    {
        using var ms = new MemoryStream();

        await Assert.ThrowsAsync<ArgumentNullException>(() => ms.WriteAsync("", null!));
    }

    private sealed class WriteOnlyStream : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => 0;
        public override long Position { get; set; }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => 0;
        public override long Seek(long offset, SeekOrigin origin) => 0;
        public override void SetLength(long value) { }
        public override void Write(byte[] buffer, int offset, int count) { }
    }
}