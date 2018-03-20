using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Bolt.Client
{
    public static class ProxyExtensions
    {
        public static void Open(this IProxy proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            proxy.OpenAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static void Close(this IProxy proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            proxy.CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static object Send(this IProxy proxy, MethodInfo action, params object[] parameters)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            return proxy.SendAsync(action, parameters).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static T Send<T>(this IProxy proxy, MethodInfo action, params object[] parameters)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            return (T)proxy.Send(action, parameters);
        }

        public static async Task<T> SendAsync<T>(this IProxy proxy, MethodInfo action, params object[] parameters)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            var result = await proxy.SendAsync(action, parameters).ConfigureAwait(false);
            return (T)result;
        }
    }
}