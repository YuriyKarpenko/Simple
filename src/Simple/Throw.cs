using System;

namespace Simple
{
    public static class Throw
    {
        /// <summary>
        /// Allows easy check the parameters
        /// </summary>
        /// <param name="value">Checking parameter</param>
        /// <param name="paramName">Parameters name for <see cref="ArgumentNullException"/></param>
        /// <param name="isValid">Addon test for paramrter</param>
        /// <returns></returns>
        public static T IsArgumentNullException<T>(T? value, string paramName, Predicate<T>? isValid = null)
        {
            if (value == null || isValid?.Invoke(value) == false)
            {
                ArgumentNullException(paramName);
            }
            return value!;
        }

        /// <summary> Create and fire a <see cref="ArgumentNullException"/> </summary>
        /// <param name="paramName"></param>
        public static void ArgumentNullException(string paramName)
            => Exception(new ArgumentNullException(paramName));

        /// <summary> A single place for exceptions to appear </summary>
        /// <param name="ex"></param>
        public static void Exception(Exception ex)
            => throw ex;
    }
}