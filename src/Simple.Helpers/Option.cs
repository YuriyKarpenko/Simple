using System;
using System.Runtime.CompilerServices;

namespace Simple.Helpers;
public static partial class Option
{
    public const string LogMsgFormat = "{0}({1}){2}";
    public const string MsgEmpty = "value is empty";
    public const string MsgInvalid = "value is not valid";
    public const string MsgNoError = "no errors!";
    public static readonly Exception ValueException = new InvalidOperationException(MsgInvalid);

    #region Create

    public static IOption<T> Try<T>(Func<T> get, [CallerMemberName] string? methodName = null)
    {
        try
        {
            return Value(get());
        }
        catch (Exception ex)
        {
            return Error<T>(ex, () => "Option.Try()", methodName);
        }
    }
    public static IOption<R> Try<T, R>(T arg, Func<T, R> select, [CallerMemberName] string? methodName = null)
        => Try(() => select(arg), methodName);

    public static IOption<T> Value<T>(T value)
        => new Option<T>(value);

    public static IOption<string> String(string? value, [CallerMemberName] string? methodName = null)
        => string.IsNullOrEmpty(value) ? Empty<string>(null, methodName) : Value<string>(value!);

    #region Error

    public static IOption<T> Error<T>(Func<string?> methodArgs, Func<string?>? methodResult = null, [CallerMemberName] string? methodName = null)
        => new Option<T>(() => MessageMethodFormat(methodArgs, methodResult, methodName));

    public static IOption<T> Error<T>(string? methodArgs, Func<string?>? methodResult = null, [CallerMemberName] string? methodName = null)
        => Error<T>(() => methodArgs, methodResult, methodName);

    public static IOption<T> Error<T>(Exception ex, Func<string?> methodArgs, [CallerMemberName] string? methodName = null)
        => Error<T>(methodArgs, () => $" => {ex.Message}\n{ex}", methodName);

    public static IOption<T> Error<T>(IOption o)
        => new Option<T>(o.GetError);

    public static IOption<T> Empty<T>(Func<string?>? methodResult = null, [CallerMemberName] string? methodName = null)
        => Error<T>(MsgEmpty, methodResult, methodName);

    #endregion

    #endregion

    //  helpers

    /// <summary> make message as $"{methodName}({methodArgs}) {methodResult}" </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string MessageMethodFormat(Func<string?> methodArgs, Func<string?>? methodResult = null, [CallerMemberName] string? methodName = null)
        => string.Format(LogMsgFormat, methodName, methodArgs(), methodResult?.Invoke());

    /// <summary> make message as $"{methodName}({methodArgs}) {methodResult}" </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string MessageMethodFormat<T>(Func<string?> methodArgs, Func<string?>? methodResult = null, [CallerMemberName] string? methodName = null)
        => MessageMethodFormat(methodArgs, methodResult, $"{methodName}<{typeof(T).Name}>");

}

public interface IOption
{
    Func<string> GetError { get; }
    bool HasValue { get; }
}

public interface IOption<out T> : IOption
{
    T Value { get; }
}

internal readonly struct Option<T> : IOption<T>
{
    private readonly T _value;
    public Option(Func<string> getError)
    {
        GetError = getError;
        _value = default!;
        HasValue = false;
    }
    public Option(T value)
    {
        GetError = () => Option.MsgNoError;
        _value = value;
        HasValue = true;
    }


    public Func<string> GetError { get; }
    public bool HasValue { get; }
    public T Value => HasValue ? _value : Throw.Exception<T>(Option.ValueException);


    public override string ToString()
        => $"{nameof(Option)}<{typeof(T).Name}>{{{HasValue} v:{_value}}}";
}