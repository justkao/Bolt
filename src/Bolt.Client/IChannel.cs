using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Bolt.Client
{
    /// <summary>
    /// Represents the type of connection to the server. This interface is used by Bolt proxies to actually send and retrieve data from server.
    /// </summary>
    public interface IChannel : IDisposable, IContractProvider
    {
        ChannelState State { get; }

        Task OpenAsync();

        Task CloseAsync();

        Task<object> SendAsync(MethodInfo action, params object[] parameters);
    }
}