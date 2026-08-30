#if NETSTANDARD
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Simple.Helpers;

//  from Microsoft.Extensions.FileSystemGlobbing, Version=2.2.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60
//  changed to a class: mutable struct had copy-mutation traps (Add on a by-value copy silently lost)
public sealed class HashCodeCombiner
{
    private long _combinedHash64;

    public int CombinedHash
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _combinedHash64.GetHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private HashCodeCombiner(long seed)
    {
        _combinedHash64 = seed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(IEnumerable e)
    {
        if (e == null)
        {
            Add(0);
            return;
        }

        var num = 0;
        foreach (var item in e)
        {
            Add(item);
            num++;
        }

        Add(num);
    }

    //  generic overload: avoids boxing elements through the non-generic IEnumerable enumerator
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add<T>(IEnumerable<T> e)
    {
        if (e == null)
        {
            Add(0);
            return;
        }

        var num = 0;
        foreach (var item in e)
        {
            Add(item is null ? 0 : item.GetHashCode());
            num++;
        }

        Add(num);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int(HashCodeCombiner self)
        => self.CombinedHash;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(int i)
        => _combinedHash64 = (_combinedHash64 << 5) + _combinedHash64 ^ i;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(string s)
    {
        var i = s?.GetHashCode() ?? 0;
        Add(i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(object o)
    {
        var i = o?.GetHashCode() ?? 0;
        Add(i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add<TValue>(TValue value, IEqualityComparer<TValue> comparer)
    {
        var i = value != null ? (comparer ?? EqualityComparer<TValue>.Default).GetHashCode(value) : 0;
        Add(i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HashCodeCombiner Start()
        => new(5381L);
}
#endif