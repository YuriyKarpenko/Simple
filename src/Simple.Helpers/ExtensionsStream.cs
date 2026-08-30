using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Helpers;

/// <summary> Extensions for <see cref="Stream"/> </summary>
public static class ExtensionsStream
{
    public static async Task<string?> ReadToEndAsync(this Stream stream)
    {
        Throw.IsArgumentNullException(stream, nameof(stream));

        if (!stream.CanRead)
        {
            return null;
        }

        if (stream.CanSeek)
        {
            stream.Seek(0, SeekOrigin.Begin);
        }

        //  leaveOpen: the caller owns the stream, we only read from it
        using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
        {
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }
    }

    public static Task WriteAsync(this Stream stream, byte[] data, CancellationToken cancellationToken = default)
    {
        Throw.IsArgumentNullException(stream, i => i.CanWrite, nameof(stream));
        Throw.IsArgumentNullException(data, nameof(data));

        return stream.WriteAsync(data, 0, data.Length, cancellationToken);
    }
    public static async Task<int> WriteAsync(this Stream stream, string content, Encoding encoding, CancellationToken ct = default)
    {
        Throw.IsArgumentNullException(stream, i => i.CanWrite, nameof(stream));
        Throw.IsArgumentNullException(encoding, nameof(encoding));

        if (string.IsNullOrEmpty(content))
        {
            return 0;
        }

        var data = encoding.GetBytes(content);
        await WriteAsync(stream, data, ct).ConfigureAwait(false);
        return data.Length;
    }
    public static Task<int> WriteAsync(this Stream stream, string content, CancellationToken cancellationToken = default)
        => WriteAsync(stream, content, Encoding.UTF8, cancellationToken);
}