using System;
using System.Collections.Generic;

namespace Simple.Helpers;

public class DicString<T> : Dictionary<string, T>
{
    public DicString()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    /// <summary> .ctor </summary>
    /// <param name="comparer">Default StringComparer.OrdinalIgnoreCase.</param>
    public DicString(IEqualityComparer<string> comparer)
        : base(comparer)
    {
    }

    public DicString(IDictionary<string, T> kvps, IEqualityComparer<string>? comparer = null)
        : base(kvps, comparer ?? StringComparer.OrdinalIgnoreCase)
    {
    }

#if !NETSTANDARD2_0
    /// <summary> .ctor </summary>
    /// <param name="kvps">Init source</param>
    /// <param name="comparer">Default StringComparer.OrdinalIgnoreCase.</param>
    public DicString(IEnumerable<KeyValuePair<string, T>> kvps, IEqualityComparer<string>? comparer = null)
        : base(kvps, comparer ?? StringComparer.OrdinalIgnoreCase)
    {
    }
#endif

    /// <summary> .ctor </summary>
    /// <param name="capacity">The initial number of elements that the Dictionary can contain</param>
    /// <param name="comparer">Default StringComparer.OrdinalIgnoreCase.</param>
    public DicString(int capacity, IEqualityComparer<string>? comparer = null)
        : base(capacity, comparer ?? StringComparer.OrdinalIgnoreCase)
    {
    }


    /// <summary> Filtering keys according to the specified enum + convert string-key to enum </summary>
    /// <typeparam name="TKey">specified enum</typeparam>
    /// <param name="ignoreCase">true to ignore case; false to consider case.</param>
    /// <returns> new expected dictionary </returns>
    public Dictionary<TKey, T> ToEnumKey<TKey>(bool ignoreCase = true) where TKey : struct, Enum
    {
        var result = new Dictionary<TKey, T>();
        foreach (var kvp in this)
        {
            //  IsDefined: Enum.TryParse accepts numeric strings like "42" even for undefined values
            if (Enum.TryParse<TKey>(kvp.Key, ignoreCase, out var k) && Enum.IsDefined(typeof(TKey), k))
            {
                result[k] = kvp.Value;
            }
        }
        return result;
    }
}