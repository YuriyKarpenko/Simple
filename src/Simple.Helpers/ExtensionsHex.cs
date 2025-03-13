using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Simple.Helpers;

public static class ExtensionsHex
{
    public const byte MaskLoByte = 0x0F;

    private static readonly char[] Ten = ['A', 'a'];

    //  Any extensions
    public static T GetServiceRequired<T>(this IServiceProvider sp)
    {
        var o = (T?)sp.GetService(typeof(T));
        return Throw.IsArgumentNullException(o, typeof(T).Name);
    }


    //  HEX extensions
    [Obsolete("this overload is exclusively for reverse compatibility")]
    public static string ToHexString(this IEnumerable<byte> src, string byteSeparator)
        => ToHexString(src.ToArray(), false, byteSeparator);
    public static string ToHexString(this IEnumerable<byte> src, bool isLowerCase = false, string byteSeparator = "")
    {
        var a = src.ToArray();
        var ss = new string[a.Length];
        //Span<char> sb = stackalloc char[a.Length * 2];

        for (var i = 0; i < a.Length; i++)
        {
            //var cc = a[i].ToHexChars(isLowerCase);
            //cc.CopyTo(ss, i*2);

            ss[i] = a[i].ToHexStr(isLowerCase);
        }

        return string.Join(byteSeparator, ss);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToHexStr(this byte b, bool isLowerCase = false)
        => new string(b.ToHexChars(isLowerCase));

#if false && NET8_0_OR_GREATER
    public static string ToHexChars(this byte b, bool isLowerCase = false) //where T : struct, IBitwiseOperators<byte, byte, byte>, IShiftOperators<byte, int, byte>
    {
        var l = ToHexChar((b as IBitwiseOperators<byte, byte, byte>) & MaskLoByte, isLowerCase);
        var h = ToHexChar(b >> 4, isLowerCase);
        return [h, l];
    }
#else
    public static char[] ToHexChars(this byte b, bool isLowerCase = false)
    {
        var l = ToHexChar(b.LoByte(), isLowerCase);
        var h = ToHexChar(b.HiByte(), isLowerCase);
        return [h, l];
    }
#endif

    public static char ToHexChar(this byte halfByte, bool isLowerCase = false)
    {
        if (halfByte < 0 || halfByte > 15)
        {
            Throw.Exception(new IndexOutOfRangeException($"ToHexChar(byte): {halfByte} must be between 0 and 15 inclusive"));
        }

        return (char)(halfByte > 9
            ? Ten[isLowerCase ? 1 : 0] + halfByte - 10
            : '0' + halfByte);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte HiByte(this byte b)
        => (byte)(b >> 4);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte LoByte(this byte b)
        => (byte)(b & MaskLoByte);
}