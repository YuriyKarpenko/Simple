using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Simple.Helpers;

public static class Throw
{
    /// <summary> Allows easy check the parameters </summary>
    /// <param name="value">Checking parameter</param>
    /// <param name="paramName">Parameters name for <see cref="ArgumentNullException"/></param>
    /// <param name="isValid">Addon test for parameter</param>
    /// <returns>Valid value</returns>
#if !NETSTANDARD
    public static T IsArgumentNullException<T>([NotNull] T? value, Predicate<T>? isValid, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        if (isValid?.Invoke(value) == false)
        {
            ArgumentException(paramName!);
        }
        return value;
    }

    public static T IsArgumentNullException<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }
#else
    public static T IsArgumentNullException<T>(T? value, Predicate<T>? isValid, string paramName)
    {
        value = IsArgumentNullException(value, paramName);

        if (isValid?.Invoke(value) == false)
        {
            ArgumentException(paramName);
        }

        return value;
    }

    public static T IsArgumentNullException<T>(T? value, string paramName)
    {
        value ??= Exception<T>(new ArgumentNullException(paramName));

        return value;
    }
#endif

    /// <summary> Create and fire a <see cref="System.ArgumentException"/> with the given <paramref name="paramName"/>. </summary>
    /// <param name="paramName">Parameter name; lands in <see cref="ArgumentException.ParamName"/>, not in the message. </param>
    public static void ArgumentException(string paramName)
        => Exception(new ArgumentException(null, paramName));

    /// <summary> Rethrow the exception preserving its original stack trace. </summary>
    [DoesNotReturn]
    public static void Exception(Exception ex)
    {
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
        throw ex; // unreachable: Throw() never returns; keeps the compiler satisfied on all TFMs
    }

    /// <summary> Rethrow the exception preserving its original stack trace. </summary>
    [DoesNotReturn]
    public static T Exception<T>(Exception ex)
    {
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
        throw ex; // unreachable: Throw() never returns; keeps the compiler satisfied on all TFMs
    }

    public static Task<T> ExceptionAsync<T>(Exception ex)
        => Task.FromException<T>(ex);
}