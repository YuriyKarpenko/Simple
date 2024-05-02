using System;
using System.Runtime.CompilerServices;

namespace Simple.Helpers
{
    public static class Throw
    {
        /// <summary> Allows easy check the parameters </summary>
        /// <param name="value">Checking parameter</param>
        /// <param name="paramName">Parameters name for <see cref="ArgumentNullException"/></param>
        /// <param name="isValid">Addon test for paramrter</param>
        /// <returns>Valid value</returns>
#if NET6_0_OR_GREATER
        public static T IsArgumentNullException<T>(T? value, Predicate<T>? isValid = null, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            ArgumentNullException.ThrowIfNull(value, paramName);
            if (isValid?.Invoke(value) == false)
            {
                ArgumentException(paramName!);
            }
            return value;
        }
#else
        public static T IsArgumentNullException<T>(T? value, string paramName, Predicate<T>? isValid = null)
        {
            if (value == null)
            {
                Exception(new ArgumentNullException(paramName));
            }

            if (isValid?.Invoke(value!) == false)
            {
                ArgumentException(paramName!);
            }

            return value!;
        }
#endif

        /// <summary> Create and fire a <see cref="ArgumentException"/> </summary>
        /// <param name="paramName"></param>
        public static void ArgumentException(string paramName)
            => Exception(new ArgumentException(paramName));

        public static void Exception(Exception ex)
            => throw ex;
    }
}