namespace System
{
    public static class ExceptionExtensions
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
    }
}