using System;
using System.Threading.Tasks;

namespace Simple.Helpers;
public static partial class Option
{
    public static IOption<T> NotNull<T>(this IOption<T?> o)
        => o.Then(i => i is null ? Empty<T>() : Value(i!));

    public static IOption<T> NotNull<T>(this IOption<T?> o) where T : struct
        => o.Then(i => i.HasValue ? Value(i.Value) : Empty<T>());

    public static IOption<C> Join<A, B, C>(this IOption<A> a, IOption<B> b, Func<A, B, IOption<C>> action)
        => a.HasValue
            ? b.HasValue
                ? action(a.Value, b.Value)
                : Error<C>(b)
            : Error<C>(a);

    public static IOption<T> Or<T>(this IOption<T> o, Func<IOption<T>> get)
        => o.HasValue ? o : get();
    public static Task<IOption<T>> OrAsync<T>(this IOption<T> o, Func<Task<IOption<T>>> getAsync)
        => o.HasValue ? Task.FromResult(o) : getAsync();

    public static IOption<T> Validate<T>(this IOption<T> o, Func<T, bool> isValid)
        => o.Then(i => isValid(i) ? o : Error<T>(MsgInvalid));

    public static T ValueOr<T>(this IOption<T> o, T alterValue)
    => o.HasValue ? o.Value : alterValue;

    public static R ValueOr<T, R>(this IOption<T> o, Func<T, R> transform, R alterValue)
    => o.HasValue ? transform(o.Value) : alterValue;


    #region Then

    public static IOption<R> Then<T, R>(this IOption<T> o, Func<T, IOption<R>> action)
        => o.HasValue ? action(o.Value) : Error<R>(o);

    public static IOption<R> ThenValue<T, R>(this IOption<T> o, Func<T, R> action)
        => o.HasValue ? Value(action(o.Value)) : Error<R>(o);

    public static IOption<R> TryThen<T, R>(this IOption<T> o, Func<T, R> action)
        => o.Then(i => Try(i, action));

    public static IOption<string> ThenStr<T>(this IOption<T> o, Func<T, string> action)
        => o.ThenValue(action);


    //  with arg1
    public static IOption<R> Then<T, T1, R>(this IOption<T> o, T1 t1, Func<T1, T, IOption<R>> action)
        => o.HasValue ? action(t1, o.Value) : Error<R>(o);

    public static IOption<T> Validate<T, T1>(this IOption<T> o, T1 t1, Func<T1, T, bool> isValid)
        => o.Then(t1, (a1, i) => isValid(a1, i) ? o : Error<T>(MsgInvalid));

    #endregion

    #region async

    //  Actionns
    public static Task ThenAsync<T>(this IOption<T> o, Func<T, Task> action)
        => o.HasValue ? action(o.Value) : Task.CompletedTask;

    public static async Task ThenAsync<T>(this Task<IOption<T>> to, Func<T, Task> action)
    {
        var o = await to;
        await o.ThenAsync(action);
    }


    //  Functions
    public static Task<IOption<R>> ThenAsync<T, R>(this IOption<T> o, Func<T, Task<IOption<R>>> select)
        => o.HasValue ? select(o.Value) : Task.FromResult(Error<R>(o));

    public static async Task<IOption<R>> ThenAsync<T, R>(this Task<IOption<T>> to, Func<T, Task<IOption<R>>> select)
    {
        var o = await to;
        return await o.ThenAsync(select);
    }

    public static async Task<IOption<R>> Then<T, R>(this Task<IOption<T>> to, Func<T, IOption<R>> select)
    {
        var o = await to;
        return o.Then(select);
    }

    public static async Task<IOption<R>> ThenValue<T, R>(this Task<IOption<T>> to, Func<T, R> select)
    {
        var o = await to;
        return o.ThenValue(select);
    }

    public static async Task<IOption<T>> OrAsync<T>(this Task<IOption<T>> to, Func<Task<IOption<T>>> getAsync)
    {
        var o = await to;
        return await o.OrAsync(getAsync);
    }


    //  Actionns with arg1
    public static Task ThenAsync<T, T1>(this IOption<T> o, T1 t1, Func<T1, T, Task> action)
        => ThenAsync(o, i => action(t1, i));

    public static Task ThenAsync<T, T1>(this Task<IOption<T>> to, T1 t1, Func<T1, T, Task> action)
        => ThenAsync(to, i => action(t1, i));


    //  Functions with arg1
    public static Task<IOption<R>> Then<T, T1, R>(this Task<IOption<T>> to, T1 t1, Func<T1, T, IOption<R>> select)
        => Then(to, i => select(t1, i));

    public static Task<IOption<R>> ThenAsync<T, T1, R>(this Task<IOption<T>> to, T1 t1, Func<T1, T, Task<IOption<R>>> select)
        => ThenAsync(to, i => select(t1, i));

    #endregion

}