using System;

namespace Bolt
{
    public static class ExceptionExtension
    {
        public static void EnsureNotCancelled(this Exception e)
        {
            if (e is OperationCanceledException)
            {
                throw e;
            }

            if (e.InnerException is OperationCanceledException)
            {
                throw e.InnerException;
            }
        }
    }
}