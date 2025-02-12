using System.Runtime.CompilerServices;
using System.Text;

namespace Simple.Helpers;

public static class StrUtil
{
    /// <summary> for simple using in linq delegates. </summary>
    public static bool NotEmpty(string? value)
        => !string.IsNullOrWhiteSpace(value);

    //  extensions

    /// <summary> GetBytes using <paramref name="encoding"/>, default Encoding.UTF8. </summary>
    /// <param name="value">Source string</param>
    /// <param name="encoding">Using encoding</param>
    /// <returns>Ecoded bytes</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] GetBytes(this string value, Encoding? encoding = null)
        => (encoding ?? Encoding.UTF8).GetBytes(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string TrimLen(this string value, uint maxLen)
        => maxLen >= value.Length
            ? value
            : maxLen < 5
                ? value.Substring(0, (int)maxLen)
                : $"{value.Substring(0, (int)maxLen - 4)} ...";


}