using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Simple.Helpers;

public static class RandomGenerator
{
    public const string CharsAllLower        = "abcdefghijklmnopqrstuvwxyz1234567890";
    public const string CharsAllUpper        = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    public const string CharsAlphabeticLower = "abcdefghijklmnopqrstuvwxyz";
    public const string CharsAlphabeticUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string CharsDigital         = "1234567890";
    public const string CharsHexLower        = "abcdef1234567890";
    public const string CharsHexUpper        = "ABCDEF1234567890";

    public enum CharOptions
    {
        AllLower,
        AllUpper,
        AlphabeticLower,
        AlphabeticUpper,
        Digital,
        HexLower,
        HexUpper,
    }

#if NETSTANDARD2_0
    private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    [ThreadStatic]
    private static byte[]? _randBuf4;

    /// <summary> Fills the buffer with cryptographically strong random bytes. </summary>
    public static void Fill(byte[] data)
        => _rng.GetBytes(data);

    private static uint NextUInt32()
    {
        var buf = _randBuf4 ??= new byte[4];
        _rng.GetBytes(buf);
        return (uint)(buf[0] | buf[1] << 8 | buf[2] << 16 | buf[3] << 24);
    }

    /// <summary> Cryptographically strong random integer in [0, toExclusive). </summary>
    public static int GetInt(int toExclusive)
    {
        if (toExclusive < 0)
        {
            Throw.Exception(new ArgumentOutOfRangeException(nameof(toExclusive)));
        }

        if (toExclusive < 2)
        {
            return 0;
        }

        //  отбор с отвержением (как в BCL RandomNumberGenerator.GetInt32):
        //  limit — наибольшее кратное range, не превышающее uint.MaxValue; в окне [0, limit)
        //  укладываются целые циклы по range, поэтому остаток % range равновероятен.
        //  Хвост [limit, 2^32) отбрасываем: иначе часть остатков встречалась бы на раз чаще (смещение модуля)
        var  range = (uint)toExclusive;
        var  limit = uint.MaxValue - uint.MaxValue % range;
        uint result;
        do
        {
            result = NextUInt32();
        } while (result >= limit); //  редкий пропуск (< range/2^32) — просто берём следующее число

        return (int)(result % range);
    }

    /// <summary> Cryptographically strong random integer in [fromInclusive, toExclusive). </summary>
    public static int GetInt(int fromInclusive, int toExclusive)
    {
        if (fromInclusive > toExclusive)
        {
            Throw.Exception(new ArgumentOutOfRangeException(nameof(toExclusive)));
        }

        var range = (long)toExclusive - fromInclusive;
        if (range <= int.MaxValue)
        {
            return fromInclusive + GetInt((int)range);
        }

        //  полный диапазон (> int.MaxValue): та же схема отбора, но в 64 битах —
        //  из двух NextUInt32 собираем ulong (полный 64-битный диапазон практически не отсекается)
        var   limit64 = ulong.MaxValue - ulong.MaxValue % (ulong)range;
        ulong result64;
        do
        {
            result64 = (ulong)NextUInt32() << 32 | NextUInt32();
        } while (result64 >= limit64); //  окно [0, limit64) кратно range → остаток равновероятен

        return (int)(fromInclusive + (long)(result64 % (ulong)range));
    }

    /// <summary> Creates a string populated with characters chosen at random from choices. </summary>
    public static string GetString(int length, CharOptions options = CharOptions.AllLower)
    {
        if (length < 0)
        {
            Throw.Exception(new ArgumentOutOfRangeException(nameof(length)));
        }

        var chars = new char[length];
        GetItemsCore(GetChoices(options).AsSpan(), chars);
        return new string(chars);
    }

#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Fill(Span<byte> data)
        => RandomNumberGenerator.Fill(data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetInt(int fromInclusive, int toExclusive)
        => RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetInt(int toExclusive)
        => RandomNumberGenerator.GetInt32(toExclusive);

    #region GetString

    /// <summary> Creates a string populated with characters chosen at random from choices. </summary>
    public static string GetString(int length, CharOptions options = CharOptions.AllLower)
        => string.Create(length, options, GetStringCore);

    private static void GetStringCore(Span<char> destination, CharOptions options)
        => GetItemsCore(GetChoices(options).AsSpan(), destination);

    #endregion

#endif

    /// <summary> Creates an array populated with items chosen at random from choices. </summary>
    public static T[] GetItems<T>(ReadOnlySpan<T> choices, uint length)
    {
        CheckEmpty(choices);

        if (length > int.MaxValue)
        {
            Throw.Exception(new ArgumentOutOfRangeException(nameof(length), length, "length must fit in Int32"));
        }

        var result = new T[length];
        GetItemsCore(choices, result);
        return result;
    }

    /// <summary>
    /// Performs an in-place shuffle of a span (Fisher–Yates).
    /// Uses <see cref="System.Security.Cryptography.RandomNumberGenerator"/> on all target frameworks.
    /// </summary>
    public static void Shuffle<T>(this Span<T> a)
    {
        //  https://ru.stackoverflow.com/questions/547996/Как-перемешать-случайно-переставить-элементы-в-массиве
        //  https://ru.wikipedia.org/wiki/Тасование_Фишера_—_Йетса
        for (var i = 0; i < a.Length - 1; i++) //  кажися в оригинале что-то напутали, 
        {
            var j = GetInt(i + 1, a.Length);
            (a[i], a[j]) = (a[j], a[i]);
        }
    }
    public static void Shuffle<T>(this Memory<T> a)
        => Shuffle(a.Span);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void CheckEmpty<T>(ReadOnlySpan<T> choices)
    {
        if (choices.IsEmpty)
        {
            Throw.Exception(new ArgumentException("choices: is empty span"));
        }
    }

    private static string GetChoices(CharOptions options)
        => options switch
        {
            CharOptions.AllLower => CharsAllLower,
            CharOptions.AllUpper => CharsAllUpper,
            CharOptions.AlphabeticLower => CharsAlphabeticLower,
            CharOptions.AlphabeticUpper => CharsAlphabeticUpper,
            CharOptions.Digital => CharsDigital,
            CharOptions.HexLower => CharsHexLower,
            CharOptions.HexUpper => CharsHexUpper,
            _ => Throw.Exception<string>(new ArgumentOutOfRangeException(nameof(options))),
        };

    private const int BatchSize = 256;

    private static void GetItemsCore<T>(ReadOnlySpan<T> choices, Span<T> destination)
    {
        var count = choices.Length;
        if (count == 1)
        {
            destination.Fill(choices[0]); //  single choice — no RNG needed at all
            return;
        }

        var countU = (uint)count;
        //  та же формула limit, что в GetInt: окно [0, limit) кратно countU → остатки равновероятны,
        //  хвост [limit, 2^32) отбрасывается (частота отсева < countU/2^32 на значение)
        var limit = uint.MaxValue - uint.MaxValue % countU;
        var i     = 0;

        //  вместо вызова GetInt на каждый элемент — один RNG-fill на BatchSize элементов:
        //  GetInt дёргал CSPRNG ради 4 байт, здесь читаем сразу 1KB сырых байт и раскладываем сами
        Span<uint> buf = stackalloc uint[BatchSize];

        while (i < destination.Length)
        {
            FillRandomBytes(MemoryMarshal.AsBytes(buf));

            for (var k = 0; k < buf.Length && i < destination.Length; k++)
            {
                var v = buf[k];
                if (v < limit) //  отсев хвоста из уже вычитанного блока — без повторного чтения RNG (как в GetInt)
                {
                    destination[i++] = choices[(int)(v % countU)];
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FillRandomBytes(Span<byte> bytes)
    {
#if NETSTANDARD2_0
        Fill(bytes.ToArray()); //  netstandard2.0: RandomNumberGenerator.GetBytes accepts byte[] only
#else
        Fill(bytes);
#endif
    }
}