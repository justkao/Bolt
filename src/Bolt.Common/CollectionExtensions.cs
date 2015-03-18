using System.Collections.Generic;
using System.Linq;

namespace Bolt.Common
{
    internal static class CollectionExtensions
    {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                return Enumerable.Empty<T>();
            }

            return source;
        }
    }
}