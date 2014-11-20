using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt
{
    public static class TaskExtensions
    {
        public static void Sleep(TimeSpan time)
        {
            Task.Delay(time).Wait();
        }

        public static T Execute<T>(Func<Task<T>> asyncFunction)
        {
            try
            {
                SynchronizationContext synchronizationContext = SynchronizationContext.Current;
                if (synchronizationContext == null)
                {
                    return asyncFunction().GetAwaiter().GetResult();
                }
                return Task.Run(() => asyncFunction().GetAwaiter().GetResult()).GetAwaiter().GetResult();
            }
            catch (AggregateException e)
            {
                throw e.Flatten().GetBaseException();
            }
        }

        public static void Execute(Func<Task> asyncFunction)
        {
            try
            {
                SynchronizationContext synchronizationContext = SynchronizationContext.Current;
                if (synchronizationContext == null)
                {
                    asyncFunction().GetAwaiter().GetResult();
                }
                else
                {
                    Task.Run(() => asyncFunction().GetAwaiter().GetResult()).GetAwaiter().GetResult();
                }
            }
            catch (AggregateException e)
            {
                throw e.Flatten().GetBaseException();
            }
        }
    }
}