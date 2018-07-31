using System;
using System.Collections.Generic;
using System.Linq;

namespace FlysasLib
{
    public static class ExtensionMethods
    {
        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

        public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

        public static int MyLength(this string str) => str == null ? 0 : str.Length;

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
    }
}
