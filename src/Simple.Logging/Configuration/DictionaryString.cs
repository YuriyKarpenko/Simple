using System;
using System.Collections;
using System.Collections.Generic;

namespace Simple.Logging.Configuration;

public class DictionaryString<T> : IDictionary<string, T>
{
    protected Dictionary<string, T> _inner = new(StringComparer.OrdinalIgnoreCase);


    #region IEnumerable

    public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        => _inner.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    #endregion

    #region ICollection

    public int Count => _inner.Count;

    public bool IsReadOnly => false;

    public void Add(KeyValuePair<string, T> item)
        => throw new NotImplementedException();

    public void Clear()
        => _inner.Clear();

    public bool Contains(KeyValuePair<string, T> item)
        => throw new NotImplementedException();

    public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        => throw new NotImplementedException();

    public bool Remove(KeyValuePair<string, T> item)
        => throw new NotImplementedException();

    #endregion

    #region IDictionary

    public virtual T this[string nameSpace]
    {
        get => _inner[nameSpace];
        set => _inner[nameSpace] = value;
    }

    public ICollection<string> Keys => _inner.Keys;

    public ICollection<T> Values => _inner.Values;


    public void Add(string key, T value)
        => this[key] = value;

    public bool ContainsKey(string key)
        => _inner.ContainsKey(key);

    public bool Remove(string key)
        => _inner.Remove(key);

    public bool TryGetValue(string key, out T value)
        => _inner.TryGetValue(key, out value!);

    #endregion

}