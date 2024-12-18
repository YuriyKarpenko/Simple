﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Helpers
{
    public static class ExtensionsHex
    {
        public static T GetServiceRequired<T>(this IServiceProvider sp)
        {
            var o = (T?)sp.GetService(typeof(T));
            return Throw.IsArgumentNullException(o, typeof(T).Name);
        }

        public static string ToHexString(this IEnumerable<byte> src, string byteSeparator = "")
        {
            var ss = src.Select(ToHexStr).ToArray();
            return string.Join(byteSeparator, ss);
        }

        public static string ToHexStr(this byte b)
        {
            var l = ToHexChar(b & 0x0F);
            var h = ToHexChar(b >> 4);
            return new string([ h, l ]);
        }

        public static char ToHexChar(this int i)
        {
            if (i < 0 || i > 15)
            {
                Throw.Exception(new IndexOutOfRangeException($"ToHexChar(int): {i} must be between 0 and 15"));
            }

            return (char)(i > 9
                ? 'A' + i - 10
                : '0' + i);
        }
    }
}