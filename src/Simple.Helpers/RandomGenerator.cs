using System;
using System.Security.Cryptography;

namespace Simple.Helpers;

public static class RandomGenerator
{
    public const string CharsAllLower = "abcdefghijklmnopqrstuvwxyz1234567890";
    public const string CharsAllUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
    public const string CharsAlphabeticLower = "abcdefghijklmnopqrstuvwxyz";
    public const string CharsAlphabeticUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string CharsDigital = "1234567890";
    public const string CharsHexLower = "abcdef1234567890";
    public const string CharsHexUpper = "ABCDEF1234567890";

#if NETSTANDARD2_0

    public static Random Random = new Random(DateTimeOffset.Now.Second);

    public static void Fill(byte[] data)
        => Random.NextBytes(data);

    public static int GetInt(int fromInclusive, int toExclusive)
        => Random.Next(fromInclusive, toExclusive);

    public static int GetInt(int toExclusive)
        => Random.Next(toExclusive);

#else

    public static void Fill(Span<byte> data)
        => RandomNumberGenerator.Fill(data);

    public static int GetInt(int fromInclusive, int toExclusive)
        => RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);

    public static int GetInt(int toExclusive)
        => RandomNumberGenerator.GetInt32(toExclusive);

    #region GetString

    public enum CharOptions
    {
        AllLower,
        AllUpper,
        AlphabeticLower,
        AlphabeticUpper,
        Digital,
        HexLower,
        HexUpper
    }

    /// <summary> Creates a string populated with characters chosen at random from choices. </summary>
    public static string GetString(int length, CharOptions options = CharOptions.AllLower)
        => string.Create(length, options, GetStringCore);


    private static void GetStringCore(Span<char> destination, CharOptions options)
    {
        ReadOnlySpan<char> choices = options switch
        {
            CharOptions.AllLower => CharsAllLower,
            CharOptions.AllUpper => CharsAllUpper,
            CharOptions.AlphabeticLower => CharsAlphabeticLower,
            CharOptions.AlphabeticUpper => CharsAlphabeticUpper,
            CharOptions.Digital => CharsDigital,
            CharOptions.HexLower => CharsHexLower,
            CharOptions.HexUpper => CharsHexUpper,
            _ => Throw.Exception<string>(new ArgumentOutOfRangeException("options"))
        };

        GetItemsCore(choices, destination);
    }

    #endregion

#endif

    /// <summary> Creates an array populated with items chosen at random from choices. </summary>
    public static T[] GetItems<T>(ReadOnlySpan<T> choices, uint length)
    {
        CheckEmpty(choices);

        var result = new T[length];
        GetItemsCore(choices, result);
        return result;
    }

    /// <summary> Performs an in-place shuffle of a span using cryptographically random number generation. </summary>
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


    private static void CheckEmpty<T>(ReadOnlySpan<T> choices)
    {
        if (choices.IsEmpty)
        {
            Throw.ArgumentException("choices: is empty span ");
        }
    }

    private static void GetItemsCore<T>(ReadOnlySpan<T> choices, Span<T> destination)
    {
        for (var i = 0; i < destination.Length; i++)
        {
            destination[i] = choices[GetInt(choices.Length)];
        }
    }
}