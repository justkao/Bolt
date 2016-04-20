
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Bolt.Test.Common
{
    public static class TestHelper
    {
        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }
    }
}