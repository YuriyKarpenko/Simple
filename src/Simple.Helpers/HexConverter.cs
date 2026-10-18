using System;

// ReSharper disable MemberCanBePrivate.Global

#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand

namespace Simple.Helpers;

public class HexConverter
{
    private static readonly sbyte[] _hexLookup = CreateLookup();

    public static byte[] AsBytes(ReadOnlySpan<byte> utf8Chars)
    {
        CheckLength(utf8Chars.Length);
        var result = new byte[utf8Chars.Length / 2];

        for (var i = 0; i < result.Length; i++)
        {
            var j = i * 2;
            var hi = _hexLookup[utf8Chars[j]];
            var lo = _hexLookup[utf8Chars[j + 1]];

            CheckHiLoBytes(lo, hi);

            result[i] = (byte)(hi << 4 | lo);
        }

        return result;
    }

    public static byte[] AsBytes(ReadOnlySpan<char> chars)
    {
        CheckLength(chars.Length);
        Span<byte> bb = stackalloc byte[chars.Length];
        for (var i = 0; i < chars.Length; i++)
        {
            bb[i] = chars[i] <= byte.MaxValue ? (byte)chars[i] : (byte)0;
        }

        return AsBytes(bb);
    }
    public static byte[] AsBytes2(ReadOnlySpan<char> chars)
    {
        CheckLength(chars.Length);
        var result = new byte[chars.Length / 2];

        for (var i = 0; i < result.Length; i++)
        {
            var j = i * 2;
            var hi = chars[j]     <= byte.MaxValue ? _hexLookup[(byte)chars[j]] : (sbyte)-1;
            var lo = chars[j + 1] <= byte.MaxValue ? _hexLookup[(byte)chars[j + 1]] : (sbyte)-1;

            CheckHiLoBytes(lo, hi);

            result[i] = (byte)(hi << 4 | lo);
        }

        return result;
    }

    public static byte[] AsBytes(string str)
        => AsBytes(str.AsSpan());

    public static char[] AsChars(ReadOnlySpan<byte> bytes, bool isLowerCase = false)
    {
        var chars = new char[bytes.Length * 2];
        var pos = 0;

        for (var i = 0; i < bytes.Length; i++)
        {
            var b = bytes[i];
            chars[pos++] = (b >> 4).ToHexChar(isLowerCase);
            chars[pos++] = (b & 0xF).ToHexChar(isLowerCase);
        }

        return chars;
    }

    public static string AsString(ReadOnlySpan<byte> bytes, bool isLowerCase = false)
        => new(AsChars(bytes, isLowerCase));

    //  private

    private static void CheckLength(int length)
    {
        if ((length & 1) != 0)
        {
            Throw.Exception(new Exception("Hex string must have even length"));
        }
    }

    private static void CheckHiLoBytes(sbyte lo, sbyte hi)
    {
        if (hi < 0 || lo < 0)
        {
            Throw.Exception(new Exception("Invalid hex character"));
        }
    }

    private static sbyte[] CreateLookup()
    {
        var table = new sbyte[256];
        for (var i = 0; i < table.Length; i++)
        {
            table[i] = -1;
        }

        for (var i = '0'; i <= '9'; i++)
        {
            table[i] = (sbyte)(i - '0');
        }

        for (var i = 'A'; i <= 'F'; i++)
        {
            table[i] = (sbyte)(10 + i - 'A');
        }

        for (var i = 'a'; i <= 'f'; i++)
        {
            table[i] = (sbyte)(10 + i - 'a');
        }

        return table;
    }
}