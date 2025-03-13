using System;
using System.Text.RegularExpressions;

namespace Simple.Helpers;

public static class ExtensionsValidation
{
    //  validation
    public static bool InRange<T>(this T value, T minInclusive, T maxInclusive) where T : IComparable
    {
        var a = value.CompareTo(minInclusive);
        var b = value.CompareTo(maxInclusive);
        return a >= 0 && b <= 0;
    }

    //  https://stackoverflow.com/questions/5342375/regex-email-validation
    public static bool IsEmail(this string? value)
        => value.NotEmpty() &&
           Regex.IsMatch(value, @"[\w\d\.\-]+@([\w\-]+)(\.\w{2,})+");

    public static bool IsIp(string ip)
    {
        const string octet0 = @"([1-9]?\d|1\d\d?|2[0-4]\d|25[0-5])";    //  allow "0" but lock "00"
        const string octet = @"([1-9]\d?|1\d\d?|2[0-4]\d|25[0-5])";     //  lock "0", "00" ..
        return Regex.IsMatch(ip, $@"^{octet}\.({octet0}\.){{2}}{octet}$");
    }
}