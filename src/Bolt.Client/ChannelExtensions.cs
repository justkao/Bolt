using System;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Client.Helpers;

namespace Bolt.Client
{
    public static class ChannelExtensions
    {
        public static void Open(this IProxy proxy)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));

            TaskHelpers.Execute(proxy.OpenAsync);
        }

        public static void Close(this IProxy proxy)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));

            TaskHelpers.Execute(proxy.CloseAsync);
        }

        public static object Send(this IProxy proxy, MethodInfo action, params object[] parameters)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));

            return TaskHelpers.Execute(() => proxy.SendAsync(action, parameters));
        }

        public static T Send<T>(this IProxy proxy, MethodInfo action, params object[] parameters)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));

            return (T) proxy.Send(action, parameters);
        }

        public static async Task<T> SendAsync<T>(this IProxy proxy, MethodInfo action, params object[] parameters)
        {
            if (proxy == null) throw new ArgumentNullException(nameof(proxy));

            var result = await proxy.SendAsync(action, parameters);
            return (T) result;
        }
    }
}