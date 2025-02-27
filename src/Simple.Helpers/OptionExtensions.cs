using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Simple.Helpers;
public static partial class Option
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> NotNull<T>(this IOption<T?> o, [CallerMemberName] string? methodName = null)
        => o.Then(i => i is null ? Empty<T>(null, methodName) : Value(i!));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> NotNull<T>(this IOption<T?> o, [CallerMemberName] string? methodName = null) where T : struct
        => o.Then(i => i.HasValue ? Value(i.Value) : Empty<T>(null, methodName));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<C> Join<A, B, C>(this IOption<A> a, IOption<B> b, Func<A, B, IOption<C>> action, [CallerMemberName] string? methodName = null)
        => a.HasValue
            ? b.HasValue
                ? action(a.Value, b.Value)
                : Error<C>(b, methodName)
            : Error<C>(a, methodName);

    #region Or

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> Or<T>(this IOption<T> o, Func<IOption<T>> get)
        => o.HasValue ? o : get();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<IOption<T>> OrAsync<T>(this IOption<T> o, Func<Task<IOption<T>>> getAsync)
        => o.HasValue ? Task.FromResult(o) : getAsync();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ValueOr<T>(this IOption<T> o, Func<T> alterGet)
        => o.HasValue ? o.Value : alterGet();

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static R ValueOr<T, R>(this IOption<T> o, Func<T, R> transform, R alterValue)
    //    => o.HasValue ? transform(o.Value) : alterValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<IOption<T>> AwaitOr<T>(this Task<IOption<T>> to, Func<IOption<T>> getAsync)
    {
        var o = await to;
        return o.Or(getAsync);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<IOption<T>> AwaitOrAsync<T>(this Task<IOption<T>> to, Func<Task<IOption<T>>> getAsync)
    {
        var o = await to;
        return await o.OrAsync(getAsync);
    }

    #endregion

    #region Then

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<R> Then<T, R>(this IOption<T> o, Func<T, IOption<R>> actionThen, Func<IOption<R>>? actionOr = null, [CallerMemberName] string? methodName = null)
        => o.HasValue ? actionThen(o.Value) : actionOr?.Invoke() ?? Error<R>(o, methodName);

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static IOption<R> ThenValue<T, R>(this IOption<T> o, Func<T, R> actionThen, [CallerMemberName] string? methodName = null)
    //    => o.HasValue ? Value(actionThen(o.Value)) : Error<R>(o, methodName);

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static IOption<R> ThenValueOr<T, R>(this IOption<T> o, Func<T, R> actionThen, Func<R> actionOr)
    //    => o.HasValue ? Value(actionThen(o.Value)) : Value(actionOr());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<R> ThenTryValue<T, R>(this IOption<T> o, Func<T, R> select, [CallerMemberName] string? methodName = null)
        => o.HasValue ? Try(o.Value, select, methodName) : Error<R>(o, methodName);


    //  with arg1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<R> Then<T, T1, R>(this IOption<T> o, T1 t1, Func<T1, T, IOption<R>> action, Func<T1, IOption<R>>? actionOr = null, [CallerMemberName] string? methodName = null)
        => o.HasValue ? action(t1, o.Value) : actionOr?.Invoke(t1) ?? Error<R>(o, methodName);

    #region async

    //  Actionns
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static Task ThenOrAsync<T>(this IOption<T> o, Func<T, Task> action, Func<Task> actionOr)
    //    => o.HasValue ? action(o.Value) : actionOr();


    //  Functions
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<IOption<R>> ThenAsync<T, R>(this IOption<T> o, Func<T, Task<IOption<R>>> select, Func<Task<IOption<R>>>? actionOr = null, [CallerMemberName] string? methodName = null)
        => o.HasValue ? select(o.Value) : actionOr?.Invoke() ?? Task.FromResult(Error<R>(o, methodName));


    ////  Actions with arg1
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static Task ThenOrAsync<T, T1>(this IOption<T> o, T1 t1, Func<T1, T, Task> actionThen, Func<T1, Task> actionOr)
    //    => o.HasValue ? actionThen(t1, o.Value) : actionOr(t1);


    //  Functions with arg1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<IOption<R>> ThenAsync<T, T1, R>(this IOption<T> o, T1 t1, Func<T1, T, Task<IOption<R>>> actionThen, Func<T1, Task<IOption<R>>>? actionOr = null, [CallerMemberName] string? methodName = null)
        => o.HasValue ? actionThen(t1, o.Value) : actionOr?.Invoke(t1) ?? Task.FromResult(Error<R>(o, methodName));

    #endregion

    #region AwaitThen

    //  Actionns
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static async Task AwaitThenOrAsync<T>(this Task<IOption<T>> to, Func<T, Task> action, Func<Task> actionOr)
    //{
    //    var o = await to;
    //    await o.ThenOrAsync(action, actionOr);
    //}

    //  Functions
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<IOption<R>> AwaitThen<T, R>(this Task<IOption<T>> to, Func<T, IOption<R>> select, Func<IOption<R>>? actionOr = null, [CallerMemberName] string? methodName = null)
    {
        var o = await to;
        return o.Then(select, actionOr, methodName);
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static async Task<IOption<R>> AwaitThenValue<T, R>(this Task<IOption<T>> to, Func<T, R> select, [CallerMemberName] string? methodName = null)
    //{
    //    var o = await to;
    //    return o.ThenValue(select, methodName);
    //}

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static async Task<IOption<R>> AwaitThenValueOr<T, R>(this Task<IOption<T>> to, Func<T, R> select, Func<R> actionOr)
    //{
    //    var o = await to;
    //    return o.ThenValueOr(select, actionOr);
    //}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<IOption<R>> AwaitThenAsync<T, R>(this Task<IOption<T>> to, Func<T, Task<IOption<R>>> select, Func<Task<IOption<R>>>? actionOr = null, [CallerMemberName] string? methodName = null)
    {
        var o = await to;
        return await o.ThenAsync(select, actionOr, methodName);
    }

    //  with arg1
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static async Task AwaitThenOrAsync<T, T1>(this Task<IOption<T>> to, T1 t1, Func<T1, T, Task> actionThen, Func<T1, Task> actionOr)
    //{
    //    var o = await to;
    //    await o.ThenOrAsync(t1, actionThen, actionOr);
    //}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<IOption<R>> AwaitThenAsync<T, T1, R>(this Task<IOption<T>> to, T1 t1, Func<T1, T, Task<IOption<R>>> select, Func<T1, Task<IOption<R>>>? actionOr = null, [CallerMemberName] string? methodName = null)
    {
        var o = await to;
        return await o.ThenAsync(t1, select, actionOr, methodName);
    }

    #endregion

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> Validate<T>(this IOption<T> o, Func<T, bool> isValid, string error = MsgInvalid, [CallerMemberName] string? methodName = null)
        => o.HasValue
            ? isValid(o.Value)
                ? o
                : Error<T>(error, null, methodName)
            : Error<T>(o, methodName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> Validate<T, T1>(this IOption<T> o, T1 t1, Func<T1, T, bool> isValid, string error = MsgInvalid, [CallerMemberName] string? methodName = null)
        => o.HasValue
            ? isValid(t1, o.Value)
                ? o
                : Error<T>(error, null, methodName)
            : Error<T>(o, methodName);
}