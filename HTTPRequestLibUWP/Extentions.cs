using System.Collections.Generic;

namespace HTTPRequestLibUWP
{
    internal static class Extentions
    {
        public static bool Contains<T>(this IEnumerable<T> collection, T item)
        {
            foreach (T entry in collection)
            {
                if (entry.Equals(item))
                    return true;
            }

            return false;
        }
    }
}
