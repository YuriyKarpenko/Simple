using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Simple.Helpers;

/// <summary>
/// Containing extensions for the collection objects.
/// </summary>
public static class ExtensionsCollection
{
    /// <summary>
    /// Returns value if <paramref name="key"/> exists and value is <typeparamref name="T"/> or <code default/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T? GetOrDefault<T>(this IDictionary<string, object> source, string key)
        => source.TryGetValue(key, out var value) && value is T t ? t : default;

    public static string AsString<T>(this IEnumerable<T> source, string separator = ", ")
        => string.Join(separator, source);

    /// <summary>
    /// Converts an <see cref="IDictionary{TKey,TValue}"/> instance to a <see cref="NameValueCollection"/> instance.
    /// </summary>
    /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> instance to convert.</param>
    /// <returns>A <see cref="NameValueCollection"/> instance.</returns>
    public static NameValueCollection ToNameValueCollection(this IDictionary<string, IEnumerable<string>> source)
    {
        var collection = new NameValueCollection(source.Count);

        foreach (var kv in source)
        {
            if (kv.Value is null)
            {
                continue; //  skip null values
            }

            foreach (var value in kv.Value)
            {
                collection.Add(kv.Key, value);
            }
        }

        return collection;
    }

    /// <summary>
    /// Merges a collection of <see cref="IDictionary{TKey,TValue}"/> instances into a single one.
    /// </summary>
    /// <param name="dictionaries">The list of <see cref="IDictionary{TKey,TValue}"/> instances to merge.</param>
    /// <returns>An <see cref="IDictionary{TKey,TValue}"/> instance containing the keys and values from the other instances.</returns>
    public static IDictionary<string, TValue> Merge<TValue>(this IEnumerable<IDictionary<string, TValue>> dictionaries)
    {
        Throw.IsArgumentNullException(dictionaries, nameof(dictionaries));

        var output = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);

        foreach (var dictionary in dictionaries.Where(d => d != null))
        {
            output.Merge(dictionary, false);
        }

        return output;
    }

#if !NET6_0_OR_GREATER
    /// <summary>
    /// Filters a collection based on a provided key selector.
    /// </summary>
    /// <param name="source">The collection filter.</param>
    /// <param name="keySelector">The predicate to filter by.</param>
    /// <typeparam name="TSource">The type of the collection to filter.</typeparam>
    /// <typeparam name="TKey">The type of the key to filter by.</typeparam>
    /// <returns>An <see cref="IEnumerable{T}"/> instance with the filtered values.</returns>
    public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
    {
        var knownKeys = new HashSet<TKey>();

        foreach (var element in source)
        {
            if (knownKeys.Add(keySelector(element)))
            {
                yield return element;
            }
        }
    }
#endif

    /// <summary>
    /// Fill <paramref name="dst"/> by key and values from <paramref name="overrideValues"/> if key not exists in <paramref name="dst"/> or <paramref name="isOverrideKeys"/> == true
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dst">dictionary to update</param>
    /// <param name="overrideValues">Source to update</param>
    /// <param name="isOverrideKeys">Is override value if key exists in <paramref name="dst"/></param>
    /// <returns>Updated <paramref name="dst"/></returns>
    public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> dst, IEnumerable<KeyValuePair<TKey, TValue>> overrideValues, bool isOverrideKeys = true)
    {
        Throw.IsArgumentNullException(dst, nameof(dst));
        Throw.IsArgumentNullException(overrideValues, nameof(overrideValues));

        foreach (var ov in overrideValues)
        {
            if (isOverrideKeys || !dst.ContainsKey(ov.Key))
            {
                dst[ov.Key] = ov.Value;
            }
        }
        return dst;
    }

    public static bool TryGet<TKey, TValue, R>(
        this IDictionary<TKey, TValue> d, TKey key,
#if !NETSTANDARD2_0
        [MaybeNullWhen(false)]
#endif
        out R result)
    {
        if (d.TryGetValue(key, out var o) && o is R r)
        {
            result = r;
            return true;
        }

        result = default!;
        return false;
    }

    public static bool TryGet<TKey, TValue, R>(
        this IReadOnlyDictionary<TKey, TValue> d, TKey key,
#if !NETSTANDARD2_0
        [MaybeNullWhen(false)]
#endif
        out R result)
    {
        if (d.TryGetValue(key, out var o) && o is R r)
        {
            result = r;
            return true;
        }

        result = default!;
        return false;
    }
}