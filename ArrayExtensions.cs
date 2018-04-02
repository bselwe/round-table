using System;
using System.Collections.Generic;
using System.Linq;

namespace RoundTable
{
    public static class ArrayExtensions
    {
        public static T[] Apply<T>(this T[] c, Func<T, int, T> a)
        {
            return c = c.Select(a).ToArray();
        }

        public static T[] Apply<T>(this T[] c, Func<T, T> a)
        {
            return c = c.Select(a).ToArray();
        }
    }
}