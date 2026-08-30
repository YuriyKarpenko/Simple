#if NETSTANDARD2_0
using System;

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary> Polyfill for netstandard2.0: the compiler recognizes this attribute by name for flow analysis. </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal sealed class DoesNotReturnAttribute : Attribute
    {
    }
}
#endif