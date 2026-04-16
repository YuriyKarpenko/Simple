using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Simple.Helpers;

public static partial class Option
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> NotNull<T>(this IOption<T?> o, string argName, [CallerMemberName] string? methodName = null)
        => o.HasValue && o.Value is null ? Error<T>(() => argName, () => MsgEmpty, methodName) : (IOption<T>)o;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> NotNull<T>(this IOption<T?> o, string argName, [CallerMemberName] string? methodName = null) where T : struct
        => o.HasValue
            ? o.Value.HasValue
                ? Value(o.Value.Value)
                : Error<T>(() => argName, () => MsgEmpty, methodName)
            : (IOption<T>)o;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<TC> Join<TA, TB, TC>(this IOption<TA> a, IOption<TB> b, Func<TA, TB, IOption<TC>> action)
        => a.HasValue
            ? b.HasValue
                ? action(a.Value, b.Value)
                : Error<TC>(b)
            : Error<TC>(a);

    #region Or

    [Obsolete]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> Or<T>(this IOption<T> o, Func<IOption<T>> get)
        => o.HasValue ? o : get();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> Or<T>(this IOption<T> o, Func<IOption<T>, IOption<T>> get)
        => o.HasValue ? o : get(o);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> Or<T, T1>(this IOption<T> o, T1 t1, Func<T1, IOption<T>, IOption<T>> get)
        => o.HasValue ? o : get(t1, o);

    [Obsolete]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<IOption<T>> OrAsync<T>(this IOption<T> o, Func<Task<IOption<T>>> getAsync)
        => o.HasValue ? Task.FromResult(o) : getAsync();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<IOption<T>> OrAsync<T>(this IOption<T> o, Func<IOption<T>, Task<IOption<T>>> getAsync)
        => o.HasValue ? Task.FromResult(o) : getAsync(o);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<IOption<T>> OrAsync<T, T1>(this IOption<T> o, T1 t1, Func<T1, IOption<T>, Task<IOption<T>>> getAsync)
        => o.HasValue ? Task.FromResult(o) : getAsync(t1, o);

    //  ValueOr
    [Obsolete]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ValueOr<T>(this IOption<T> o, Func<T> alterGet)
        => o.HasValue ? o.Value : alterGet();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ValueOr<T>(this IOption<T> o, Func<IOption<T>, T> alterGet)
        => o.HasValue ? o.Value : alterGet(o);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> ValueOrAsync<T>(this IOption<T> o, Func<IOption<T>, Task<T>> factory)
    {
        var t = o.HasValue ? o.Value : await factory(o).ConfigureAwait(false);
        return t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> ValueOrAsync<T1, T>(this IOption<T> o, T1 t1, Func<T1, IOption<T>, Task<T>> factory)
    {
        var t = o.HasValue ? o.Value : await factory(t1, o).ConfigureAwait(false);
        return t;
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static R ValueOr<T, R>(this IOption<T> o, Func<T, R> transform, R alterValue)
    //    => o.HasValue ? transform(o.Value) : alterValue;


    [Obsolete]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<IOption<T>> AwaitOr<T>(this Task<IOption<T>> to, Func<IOption<T>> getAsync)
    {
        var o = await to;
        return o.Or(getAsync);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<IOption<T>> AwaitOr<T>(this Task<IOption<T>> to, Func<IOption<T>, IOption<T>> getAsync)
    {
        var o = await to;
        return o.Or(getAsync);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> AwaitValueOr<T>(this Task<IOption<T>> to, T defValue)
    {
        var o = await to;
        return o.ValueOr(_ => defValue);
    }

    [Obsolete]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<IOption<T>> AwaitOrAsync<T>(this Task<IOption<T>> to, Func<Task<IOption<T>>> getAsync)
    {
        var o = await to;
        return await o.OrAsync(getAsync);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<IOption<T>> AwaitOrAsync<T>(this Task<IOption<T>> to, Func<IOption<T>, Task<IOption<T>>> getAsync)
    {
        var o = await to;
        return await o.OrAsync(getAsync);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<T> AwaitValueOr<T>(this Task<IOption<T>> to, Func<IOption<T>, T> factory)
    {
        var o = await to;
        return o.ValueOr(factory);
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // public static async Task<T> AwaitValueOr<T1, T>(this Task<IOption<T>> to, T1 t1, Func<T1, IOption<T>, Task<T>> factory)
    // {
    //     var o = await to;
    //     return o.ValueOr(t1, factory);
    // }

    #endregion

    #region Then

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<R> Then<T, R>(this IOption<T> o, Func<T, IOption<R>> actionThen, Func<IOption<T>, IOption<R>>? actionOr = null)
        => o.HasValue ? actionThen(o.Value) : actionOr?.Invoke(o) ?? Error<R>(o);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<R> ThenTryValue<T, R>(this IOption<T> o, Func<T, R> select, [CallerMemberName] string? methodName = null)
        => o.HasValue ? Try(o.Value, select, methodName) : Error<R>(o);


    //  with arg1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<R> Then<T, T1, R>(this IOption<T> o, T1 t1, Func<T1, T, IOption<R>> action, Func<T1, IOption<T>, IOption<R>>? actionOr = null)
        => o.HasValue ? action(t1, o.Value) : actionOr?.Invoke(t1, o) ?? Error<R>(o);

    #region async

    //  Functions
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<IOption<R>> ThenAsync<T, R>(this IOption<T> o, Func<T, Task<IOption<R>>> select, Func<IOption<T>, Task<IOption<R>>>? actionOr = null)
        => o.HasValue ? select(o.Value) : actionOr?.Invoke(o) ?? Task.FromResult(Error<R>(o));


    ////  Actions with arg1
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static Task ThenOrAsync<T, T1>(this IOption<T> o, T1 t1, Func<T1, T, Task> actionThen, Func<T1, Task> actionOr)
    //    => o.HasValue ? actionThen(t1, o.Value) : actionOr(t1);


    //  Functions with arg1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<IOption<R>> ThenAsync<T, T1, R>(this IOption<T> o, T1 t1, Func<T1, T, Task<IOption<R>>> actionThen, Func<T1, IOption<T>, Task<IOption<R>>>? actionOr = null)
        => o.HasValue ? actionThen(t1, o.Value) : actionOr?.Invoke(t1, o) ?? Task.FromResult(Error<R>(o));

    #endregion

    #region ThenAction

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThenAction<T>(this IOption<T> o, Action<T> actOk, Action<IOption<T>>? actOr = null)
    {
        if (o.HasValue)
        {
            actOk(o.Value);
        }
        else
        {
            actOr?.Invoke(o);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThenAction<T, TArg>(this IOption<T> o, TArg arg, Action<TArg, T> action, Action<TArg, IOption<T>>? actOr = null)
    {
        if (o.HasValue)
        {
            action(arg, o.Value);
        }
        else
        {
            actOr?.Invoke(arg, o);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task ThenActionAsync<T>(this IOption<T> o, Func<T, Task> action, Func<IOption<T>, Task>? actOr = null)
    {
        var t = o.HasValue ? action(o.Value) : actOr?.Invoke(o) ?? Task.CompletedTask;
        await t.ConfigureAwait(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task ThenActionAsync<T, TArg>(this IOption<T> o, TArg arg, Func<TArg, T, Task> action, Func<TArg, IOption<T>, Task>? actOr = null)
    {
        var t = o.HasValue ? action(arg, o.Value) : actOr?.Invoke(arg, o) ?? Task.CompletedTask;
        await t.ConfigureAwait(false);
    }

    #endregion

    #region Value

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<R> ThenValue<T, R>(this IOption<T> o, Func<T, R> select, Func<IOption<T>, R>? actOr = null)
        => o.Then(i => Value(select(i)), actOr == null ? null : i => Value(actOr(i)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<R> ThenValue<T, T1, R>(this IOption<T> o, T1 t1, Func<T1, T, R> select, Func<T1, IOption<T>, R>? actOr = null)
        => o.Then(i => Value(select(t1, i)), actOr == null ? null : i => Value(actOr(t1, i)));

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
    public static async Task<IOption<R>> AwaitThen<T, R>(this Task<IOption<T>> to, Func<T, IOption<R>> select, Func<IOption<T>, IOption<R>>? actionOr = null)
    {
        var o = await to;
        return o.Then(select, actionOr);
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
    public static async Task<IOption<R>> AwaitThenAsync<T, R>(this Task<IOption<T>> to, Func<T, Task<IOption<R>>> select, Func<IOption<T>, Task<IOption<R>>>? actionOr = null)
    {
        var o = await to;
        return await o.ThenAsync(select, actionOr);
    }

    //  with arg1
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static async Task AwaitThenOrAsync<T, T1>(this Task<IOption<T>> to, T1 t1, Func<T1, T, Task> actionThen, Func<T1, Task> actionOr)
    //{
    //    var o = await to;
    //    await o.ThenOrAsync(t1, actionThen, actionOr);
    //}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async Task<IOption<R>> AwaitThenAsync<T, T1, R>(this Task<IOption<T>> to, T1 t1, Func<T1, T, Task<IOption<R>>> select, Func<T1, IOption<T>, Task<IOption<R>>>? actionOr = null)
    {
        var o = await to;
        return await o.ThenAsync(t1, select, actionOr);
    }

    #endregion

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> Validate<T>(this IOption<T> o, Func<T, bool> isValid, string argName, string error = MsgInvalid, [CallerMemberName] string? methodName = null)
        => o.HasValue && !isValid(o.Value)
            ? Error<T>(() => argName, () => error, methodName)
            : o;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> Validate<T, T1>(this IOption<T> o, T1 t1, Func<T1, T, bool> isValid, string argName, string error = MsgInvalid, [CallerMemberName] string? methodName = null)
        => o.HasValue && !isValid(t1, o.Value)
            ? Error<T>(() => argName, () => error, methodName)
            : o;
}