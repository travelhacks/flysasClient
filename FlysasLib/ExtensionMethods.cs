﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace FlysasLib
{
    public static class ExtensionMethods
    {
        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

        public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

        public static int MyLength(this string str) => str == null ? 0 : str.Length;

        public static string MyTrim(this string str) => str?.Trim();

        public static string camelCase(this string str)
        {
            if (str.IsNullOrEmpty())
                return string.Empty;
            return str.First().ToString().ToLower() + str.Substring(1);
        }

        static IEnumerable<string> simplify(IEnumerable<string> list)
        {
            if (list.Distinct().Count() == 1)
                yield return list.First();
            else
                foreach (string s in list)
                    yield return s;
        }
        public static String SimplifyAndJoin(this IEnumerable<string> list, string separator)
        {
            return string.Join(separator, simplify(list));
        }

        public static String PostPendIfNotEmpty(this string s, string post)
        {
            if (s.IsNullOrEmpty())
                return s;
            return s + post;
        }

        

        public static int MyDayOfWeek(this DateTime dt)
        {
            if (dt.DayOfWeek == DayOfWeek.Sunday)
                return 7;
            return (int)dt.DayOfWeek;
        }

        public static void Shuffle<T>(this List<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
