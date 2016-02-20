using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client.Helpers
{
    internal static class TaskHelpers
    {
        public static T Execute<T>(Func<Task<T>> asyncFunction)
        {
            SynchronizationContext synchronizationContext = SynchronizationContext.Current;
            if (synchronizationContext == null)
            {
                return asyncFunction().GetAwaiter().GetResult();
            }

            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                return asyncFunction().GetAwaiter().GetResult();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            }
        }

        public static void Execute(Func<Task> asyncFunction)
        {
            SynchronizationContext synchronizationContext = SynchronizationContext.Current;
            if (synchronizationContext == null)
            {
                asyncFunction().GetAwaiter().GetResult();
                return;
            }

            SynchronizationContext.SetSynchronizationContext(null);
            try
            {
                asyncFunction().GetAwaiter().GetResult();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
            }
        }
    }
}