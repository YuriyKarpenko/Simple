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
    public static IOption<C> Join<A, B, C>(this IOption<A> a, IOption<B> b, Func<A, B, IOption<C>> action)
        => a.HasValue
            ? b.HasValue
                ? action(a.Value, b.Value)
                : Error<C>(b)
            : Error<C>(a);

    #region Or

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> Or<T>(this IOption<T> o, Func<IOption<T>> get)
        => o.HasValue ? o : get();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> Or<T>(this IOption<T> o, Func<IOption<T>, IOption<T>> get)
        => o.HasValue ? o : get(o);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> Or<T, T1>(this IOption<T> o, T1 t1, Func<T1, IOption<T>, IOption<T>> get)
        => o.HasValue ? o : get(t1, o);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<IOption<T>> OrAsync<T>(this IOption<T> o, Func<Task<IOption<T>>> getAsync)
        => o.HasValue ? Task.FromResult(o) : getAsync();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<IOption<T>> OrAsync<T>(this IOption<T> o, Func<IOption<T>, Task<IOption<T>>> getAsync)
        => o.HasValue ? Task.FromResult(o) : getAsync(o);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<IOption<T>> OrAsync<T, T1>(this IOption<T> o, T1 t1, Func<T1, IOption<T>, Task<IOption<T>>> getAsync)
        => o.HasValue ? Task.FromResult(o) : getAsync(t1, o);

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
    public static IOption<R> Then<T, R>(this IOption<T> o, Func<T, IOption<R>> actionThen, Func<IOption<T>, IOption<R>>? actionOr = null)
        => o.HasValue ? actionThen(o.Value) : actionOr?.Invoke(o) ?? Error<R>(o);

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static IOption<R> ThenValue<T, R>(this IOption<T> o, Func<T, R> actionThen, [CallerMemberName] string? methodName = null)
    //    => o.HasValue ? Value(actionThen(o.Value)) : Error<R>(o, methodName);

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static IOption<R> ThenValueOr<T, R>(this IOption<T> o, Func<T, R> actionThen, Func<R> actionOr)
    //    => o.HasValue ? Value(actionThen(o.Value)) : Value(actionOr());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<R> ThenTryValue<T, R>(this IOption<T> o, Func<T, R> select, [CallerMemberName] string? methodName = null)
        => o.HasValue ? Try(o.Value, select, methodName) : Error<R>(o);


    //  with arg1
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<R> Then<T, T1, R>(this IOption<T> o, T1 t1, Func<T1, T, IOption<R>> action, Func<T1, IOption<T>, IOption<R>>? actionOr = null)
        => o.HasValue ? action(t1, o.Value) : actionOr?.Invoke(t1, o) ?? Error<R>(o);

    #region async

    //  Actionns
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static Task ThenOrAsync<T>(this IOption<T> o, Func<T, Task> action, Func<Task> actionOr)
    //    => o.HasValue ? action(o.Value) : actionOr();


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
            ? Error<T>(argName, () => error, methodName)
            : o;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IOption<T> Validate<T, T1>(this IOption<T> o, T1 t1, Func<T1, T, bool> isValid, string argName, string error = MsgInvalid, [CallerMemberName] string? methodName = null)
        => o.HasValue && !isValid(t1, o.Value)
            ? Error<T>(argName, () => error, methodName)
            : o;
}