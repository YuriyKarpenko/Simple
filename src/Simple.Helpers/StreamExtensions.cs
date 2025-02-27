using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Helpers;

/// <summary> Extensions for <see cref="Stream"/> </summary>
public static class StreamExtensions
{
    public static async Task<string?> ReadToEndAsync(this Stream? stream)
    {
        Throw.IsArgumentNullException(stream, nameof(stream));

        if (stream?.CanRead != true)
        {
            return null;
        }

        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, false))
        {
            return await reader.ReadToEndAsync();
        }
    }

    [Obsolete]
    public static ArraySegment<byte> GetBufferSegment(this MemoryStream stream)
    {
        return stream.TryGetBuffer(out var buffer)
            ? buffer
            : default;
    }

    public static Task WriteAsync(this Stream stream, byte[] data, CancellationToken cancellationToken = default)
    {
        Throw.IsArgumentNullException(stream, i => i.CanWrite, nameof(stream));

        return stream.WriteAsync(data, 0, data.Length, cancellationToken);
    }
    public static async Task<int> WriteAsync(this Stream stream, string content, Encoding encoding, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(content))
        {
            return 0;
        }

        Throw.IsArgumentNullException(encoding, nameof(encoding));

        var data = encoding.GetBytes(content);
        await WriteAsync(stream, data, cancellationToken);
        return data.Length;
    }
    public static Task<int> WriteAsync(this Stream stream, string content, CancellationToken cancellationToken = default)
        => WriteAsync(stream, content, Encoding.UTF8, cancellationToken);
}