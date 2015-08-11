using System;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Client.Helpers;

namespace Bolt.Client
{
    public static class ChannelExtensions
    {
        public static void Open(this IChannel channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            TaskHelpers.Execute(channel.OpenAsync);
        }

        public static void Close(this IChannel channel)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            TaskHelpers.Execute(channel.CloseAsync);
        }

        public static object Send(this IChannel channel, MethodInfo action, params object[] parameters)
        {
            return TaskHelpers.Execute(() => channel.SendAsync(action, parameters));
        }

        public static T Send<T>(this IChannel channel, MethodInfo action, params object[] parameters)
        {
            return (T) channel.Send(action, parameters);
        }

        public static async Task<T> SendAsync<T>(this IChannel channel, MethodInfo action, params object[] parameters)
        {
            var result = await channel.SendAsync(action, parameters);
            return (T) result;
        }
    }
}