using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Simple.Helpers
{
    /// <summary>
    /// Containing extensions for the collection objects.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Returns value if <paramref name="key"/> exists and value is <typeparamref name="T"/> or <code default/>
        /// </summary>
        public static T? GetOrDefault<T>(this IDictionary<string, object> source, string key)
            => source.TryGetValue(key, out var value) && value is T t ? t : default;

        public static string AsString<T>(this IEnumerable<T> souce, string separator = ", ")
            => string.Join(separator, souce);

        /// <summary>
        /// Converts an <see cref="IDictionary{TKey,TValue}"/> instance to a <see cref="NameValueCollection"/> instance.
        /// </summary>
        /// <param name="source">The <see cref="IDictionary{TKey,TValue}"/> instance to convert.</param>
        /// <returns>A <see cref="NameValueCollection"/> instance.</returns>
        public static NameValueCollection ToNameValueCollection(this IDictionary<string, IEnumerable<string>> source)
        {
            var collection = new NameValueCollection();

            foreach (var key in source.Keys)
            {
                foreach (var value in source[key])
                {
                    collection.Add(key, value);
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
            var output = new Dictionary<string, TValue>(StringComparer.OrdinalIgnoreCase);

            foreach (var dictionary in dictionaries.Where(d => d != null))
            {
                output.Merge(dictionary, false);
            }

            return output;
        }

        /// <summary>
        /// Filters a collection based on a provided key selector.
        /// </summary>
        /// <param name="source">The collection filter.</param>
        /// <param name="keySelector">The predicate to filter by.</param>
        /// <typeparam name="TSource">The type of the collection to filter.</typeparam>
        /// <typeparam name="TKey">The type of the key to filter by.</typeparam>
        /// <returns>A <see cref="IEnumerable{T}"/> instance with the filtered values.</returns>
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

        /// <summary>
        /// Fill <paramref name="dst"/> by key and values from <paramref name="overrideValues"/> if key not exists in <paramref name="dst"/> or <paramref name="isOverrideKeys"/> == true
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dst">dictionary to update</param>
        /// <param name="overrideValues">Source to update</param>
        /// <param name="isOverrideKeys">Is override value if key exists in <paramref name="dst"/></param>
        /// <returns>Updated <paramref name="dst"/></returns>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> dst, IDictionary<TKey, TValue> overrideValues, bool isOverrideKeys = true)
        {
            foreach (var key in overrideValues.Keys)
            {
                if (isOverrideKeys || !dst.ContainsKey(key))
                {
                    dst[key] = overrideValues[key];
                }
            }
            return dst;
        }
    }
}