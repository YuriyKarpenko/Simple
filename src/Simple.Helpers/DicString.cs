using System;
using System.Collections.Generic;

namespace Simple.Helpers;

public class DicString<T> : Dictionary<string, T>
{
    /// <summary> .ctor </summary>
    /// <param name="comparer">Default StringComparer.InvariantCultureIgnoreCase.</param>
    public DicString(IEqualityComparer<string>? comparer = null)
        : base(comparer ?? StringComparer.InvariantCultureIgnoreCase) { }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary> .ctor </summary>
    /// <param name="kvps">Init source</param>
    /// <param name="comparer">Default StringComparer.InvariantCultureIgnoreCase.</param>
    public DicString(IEnumerable<KeyValuePair<string, T>> kvps, IEqualityComparer<string>? comparer = null)
        : base(kvps, comparer ?? StringComparer.InvariantCultureIgnoreCase) { }
#endif

    /// <summary> .ctor </summary>
    /// <param name="capacity">The initial number of elements that the Dictionary can contain</param>
    /// <param name="comparer">Default StringComparer.InvariantCultureIgnoreCase.</param>
    public DicString(int capacity, IEqualityComparer<string>? comparer = null)
        : base(capacity, comparer ?? StringComparer.InvariantCultureIgnoreCase) { }


    /// <summary> Filtering keys according to the specified enum + convert string-key to enum </summary>
    /// <typeparam name="TKey">specified enum</typeparam>
    /// <param name="ignoreCase">true to ignore case; false to consider case.</param>
    /// <returns> new expected dictionary </returns>
    public Dictionary<TKey, T> ToEnumKey<TKey>(bool ignoreCase = true) where TKey : struct
    {
        var result = new Dictionary<TKey, T>();
        foreach (var kvp in this)
        {
            if (Enum.TryParse<TKey>(kvp.Key, ignoreCase, out var k))
            {
                result[k] = kvp.Value;
            }
        }
        return result;
    }
}