using System;
using System.Collections.Generic;
using System.Linq;

namespace FlysasClient
{
    public static class ExtensionMethods
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
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
