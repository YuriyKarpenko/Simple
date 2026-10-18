using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Simple.Helpers;

public static class ExtensionsHex
{
    public const byte MaskLoByte = 0x0F;

    private static readonly char[] Ten = ['A', 'a'];

    //  Any extensions
    public static object GetServiceRequired(this IServiceProvider sp, Type t)
    {
        var o = sp.GetService(t);
        return Throw.IsArgumentNullException(o, t.Name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetServiceRequired<T>(this IServiceProvider sp)
        => (T)GetServiceRequired(sp, typeof(T));


    //  HEX extensions
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte HiByte(this byte b)
        => (byte)(b >> 4);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte LoByte(this byte b)
        => (byte)(b & MaskLoByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToHexChar(this byte b, bool isLowerCase = false)
    {
        if (b is < 0 or > 15)
        {
            Throw.Exception(new IndexOutOfRangeException($"ToHexChar(byte): {b} must be between 0 and 15 inclusive"));
        }

        return (char)(b > 9
            ? b + Ten[isLowerCase ? 1 : 0] - 10
            : b + '0');
    }

    public static char[] ToHexChars(this byte b, bool isLowerCase = false)
    {
        var l = ToHexChar(b & MaskLoByte, isLowerCase);
        var h = ToHexChar(b >> 4, isLowerCase);
        return [h, l];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexStr(this byte b, bool isLowerCase = false)
        => new(b.ToHexChars(isLowerCase));

    public static string ToHexString(this IEnumerable<byte> src, bool isLowerCase = false, string byteSeparator = "")
    {
        var a = src as byte[] ?? src.ToArray();
        if (a.Length == 0)
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(byteSeparator))
        {
            return HexConverter.AsString(a, isLowerCase);
        }

        var sb = new StringBuilder(a.Length * 2 + (a.Length - 1) * byteSeparator.Length);
        for (var i = 0; i < a.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(byteSeparator);
            }

            sb.Append(a[i].ToHexChars(isLowerCase));
        }

        return sb.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte HiByte(this int asByte)
        => (byte)(asByte >> 4);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte LoByte(this int asByte)
        => (byte)(asByte & MaskLoByte);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char ToHexChar(this int asByte, bool isLowerCase = false)
        => ToHexChar((byte)asByte, isLowerCase);
}