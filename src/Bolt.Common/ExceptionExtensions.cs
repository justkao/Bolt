using System.Collections.Generic;

namespace System
{
    internal static class ExceptionExtensions
    {
        public static void EnsureNotCancelled(this Exception e)
        {
            while (e != null)
            {
                if (e is OperationCanceledException)
                {
                    throw e;
                }

                e = e.InnerException;
            }
        }

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