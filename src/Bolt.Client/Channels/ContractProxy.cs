using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Bolt.Core;

namespace Bolt.Client.Channels
{
    /// <summary>
    /// Base class for all Bolt generated proxies. The <see cref="ContractProxy"/> requires instance of <see cref="IChannel"/> that is used to communicate with Bolt server.
    /// </summary>
    public abstract class ContractProxy : IContractProvider, IChannel
    {
        protected ContractProxy()
        {
        }

        protected ContractProxy(ContractProxy proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException(nameof(proxy));
            }

            Contract = proxy.Contract;
            Channel = proxy.Channel;
        }

        /// <summary>
        /// Initializes the instance will the channel and contract descriptor.
        /// </summary>
        /// <param name="contract">The contract descriptor.</param>
        /// <param name="channel">The channel used to communicate with bolt server.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of parameters is null.</exception>
        protected ContractProxy(Type contract, IChannel channel)
        {
            if (contract == null)
            {
                throw new ArgumentNullException(nameof(contract));
            }

            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            Contract = contract;
            Channel = channel;
        }

        /// <summary>
        /// Gets the contract descriptor that describes the available proxy actions.
        /// </summary>
        public Type Contract { get; protected set; }

        /// <summary>
        /// The channel used to communicate with Bolt server.
        /// </summary>
        public IChannel Channel { get; protected set; }

        #region IChannel Implementation

        public void Dispose()
        {
            Channel.Dispose();
        }

        void IChannel.Open()
        {
            Channel.Open();
        }

        Task IChannel.OpenAsync()
        {
            return Channel.OpenAsync();
        }

        bool IChannel.IsOpened => Channel.IsOpened;

        ISerializer IChannel.Serializer => Channel.Serializer;

        void ICloseable.Close()
        {
            Channel.Close();
        }

        Task ICloseable.CloseAsync()
        {
            return Channel.CloseAsync();
        }

        bool ICloseable.IsClosed => Channel.IsClosed;

        Task<object> IChannel.SendAsync(Type contract, MethodInfo action, Type resultType, IObjectSerializer parameters, CancellationToken cancellation)
        {
            return Channel.SendAsync(contract, action, resultType, parameters, cancellation);
        }

        protected Task SendAsync(MethodInfo action, IObjectSerializer parameters, CancellationToken cancellation)
        {
            return SendAsync<Empty>(action, parameters, cancellation);
        }

        protected async Task<TResult> SendAsync<TResult>(MethodInfo action, IObjectSerializer parameters, CancellationToken cancellation)
        {
            object result =  await Channel.SendAsync(Contract, action, typeof(TResult), parameters, cancellation);
            return (TResult)result;
        }

        protected void Send(MethodInfo action, IObjectSerializer parameters, CancellationToken cancellation)
        {
            TaskHelpers.Execute(() => SendAsync<Empty>(action, parameters, cancellation));
        }

        protected TResult Send<TResult>(
            MethodInfo action,
            IObjectSerializer parameters,
            CancellationToken cancellation)
        {
            return TaskHelpers.Execute(() => SendAsync<TResult>(action, parameters, cancellation));
        }

        #endregion
    }
}