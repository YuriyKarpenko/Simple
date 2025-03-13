using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Simple.Helpers;

public static class StrUtil
{
    //  validation

    /// <summary> for simple using in linq delegates. </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NotEmpty(
#if !NETSTANDARD2_0
        [NotNullWhen(true)] 
#endif
    this string? value)
        => !string.IsNullOrWhiteSpace(value);

    //  utils

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

    public static string GetClassName(this Type t)
    {
        if (t.IsGenericType)
        {
            var suffix = t.GenericTypeArguments.Select(GetClassName).AsString();
            var a = t.Name.AsSpan();
            var end = a.IndexOf('`');

            return end > 0
                ? $"{new string(a.ToArray(), 0, end)}<{suffix}>"
                : $"{t.Name}<{suffix}>";
        }

        return t.Name;
    }
}