using System;
using System.Collections.Generic;

namespace Bolt.Common
{
    internal static class ExceptionExtensions
    {
        public static IEnumerable<Exception> GetAll(this Exception e)
        {
            while (e != null)
            {
                yield return e;
                e = e.InnerException;
            }
        }
    }
}