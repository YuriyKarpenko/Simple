using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Simple.Helpers;

/*
I: я обратил внимание, что ты заменил подстановки "\d" на "[0-9]", в этом есть преимущества ?
AI:
\d в .NET (без RegexOptions.ECMAScript) матчит любой десятичный символ Unicode (категория Nd), а не только ASCII 0–9. Например, валидацию пройдут строки вроде:

١٢٣.٤٥٦.٧٨٩.٠١٢ (арабо-индийские цифры)
１２３.４５６.７８９.０１２ (полные/широкие цифры)
деванагари, тамильские и т.д.
То есть IsIp("١٢٣.٤٥٦.٧٨٩.٠١٢") вернул бы true. Это нежелательно по двум причинам:

Расхождение валидатора и парсера. Валидация пропускает строку, а потребитель (например, IPAddress.Parse, бэкенд, сетевой стек) может её отвергнуть или интерпретировать иначе — классический источник «валидация прошла, а дальше упало/инъекция прошла».
IP-адрес по определению ASCII. Unicode-цифры в octet-частях не имеют смысла — это только ложные срабатывания.
[0-9] явно ограничивает класс символов ASCII. Альтернатива — RegexOptions.ECMAScript (тогда \d = ASCII), но это свойство опций, легко забыть при копировании паттерна, и неявно для читателя.
*/
public static class ExtensionsValidation
{
    private static readonly TimeSpan RegExTtl = TimeSpan.FromSeconds(1);
    //  validation
    public static bool InRange<T>(this T value, T minInclusive, T maxInclusive) where T : IComparable
    {
        var comparer = Comparer<T>.Default;
        var a        = comparer.Compare(value, minInclusive);
        var b        = comparer.Compare(value, maxInclusive);
        return a >= 0 && b <= 0;
    }

    //  https://stackoverflow.com/questions/5342375/regex-email-validation
    //  ASCII-only: explicit char classes (like IsIp) — \w/\d would accept Unicode digits/letters
    private static readonly Regex _emailRegex = new(
        @"^[a-zA-Z0-9_.\-]+@([a-zA-Z0-9_\-]+)(\.[a-zA-Z0-9_]{2,})+\z",
        RegexOptions.CultureInvariant,
        RegExTtl);

    public static bool IsEmail(this string? value)
        => value.NotEmpty() && _emailRegex.IsMatch(value);

    private static readonly Regex _ipRegex = new(
        @"^([1-9][0-9]?|1[0-9][0-9]?|2[0-4][0-9]|25[0-5])\.(([1-9]?[0-9]|1[0-9][0-9]?|2[0-4][0-9]|25[0-5])\.){2}([1-9][0-9]?|1[0-9][0-9]?|2[0-4][0-9]|25[0-5])\z",
        RegexOptions.CultureInvariant,
        RegExTtl);

    public static bool IsIp(this string? ip)
        => !string.IsNullOrEmpty(ip) && _ipRegex.IsMatch(ip);
}