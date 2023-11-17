using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Helpers
{
    /// <summary>
    /// Extensions for <see cref="MemoryStream"/>
    /// </summary>
    public static class StreamExtensions
    {
        public static async Task<string?> ReadToEndAsync(this Stream stream)
        {
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

        /// <summary>
        /// Gets the buffer segment.
        /// </summary>
        /// <param name="stream">The memory stream.</param>
        /// <returns>Buffer segment as bytes</returns>
        public static ArraySegment<byte> GetBufferSegment(this MemoryStream stream)
        {
            if (stream.TryGetBuffer(out var buffer))
            {
                return buffer;
            }
            return default;
        }
    }
}