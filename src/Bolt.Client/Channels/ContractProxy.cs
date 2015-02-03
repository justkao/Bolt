using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bolt.Client.Channels
{
    /// <summary>
    /// Base class for all Bolt generated proxies. The <see cref="ContractProxy"/> requires instance of <see cref="IChannel"/> that is used to communicate with Bolt server.
    /// </summary>
    public abstract class ContractProxy : IContractDescriptorProvider, IChannel
    {
        protected ContractProxy(ContractProxy proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException("proxy");
            }

            Descriptor = proxy.Descriptor;
            Channel = proxy.Channel;
        }

        /// <summary>
        /// Initializes the instance will the channel and contract descriptor.
        /// </summary>
        /// <param name="contractDescriptor">The contract descriptor.</param>
        /// <param name="channel">The channel used to communicate with bolt server.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of parameters is null.</exception>
        protected ContractProxy(ContractDescriptor contractDescriptor, IChannel channel)
        {
            if (contractDescriptor == null)
            {
                throw new ArgumentNullException("contractDescriptor");
            }

            if (channel == null)
            {
                throw new ArgumentNullException("channel");
            }

            Descriptor = contractDescriptor;
            Channel = channel;
        }

        /// <summary>
        /// Gets the contract descriptor that describes the available proxy actions.
        /// </summary>
        public ContractDescriptor Descriptor { get; protected set; }

        /// <summary>
        /// The channel used to communicate with Bolt server.
        /// </summary>
        public IChannel Channel { get; private set; }

        #region IChannel Implementation

        public virtual CancellationToken GetCancellationToken(ActionDescriptor descriptor)
        {
            return Channel.GetCancellationToken(descriptor);
        }

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

        bool IChannel.IsOpened
        {
            get { return Channel.IsOpened; }
        }

        void ICloseable.Close()
        {
            Channel.Close();
        }

        Task ICloseable.CloseAsync()
        {
            return Channel.CloseAsync();
        }

        bool ICloseable.IsClosed
        {
            get { return Channel.IsClosed; }
        }

        Task IChannel.SendAsync<TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            return Channel.SendAsync(parameters, descriptor, cancellation);
        }

        Task<TResult> IChannel.SendAsync<TResult, TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            return Channel.SendAsync<TResult, TRequestParameters>(parameters, descriptor, cancellation);
        }

        void IChannel.Send<TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            Channel.Send(parameters, descriptor, cancellation);
        }

        TResult IChannel.Send<TResult, TRequestParameters>(TRequestParameters parameters, ActionDescriptor descriptor, CancellationToken cancellation)
        {
            return Channel.Send<TResult, TRequestParameters>(parameters, descriptor, cancellation);
        }

        #endregion
    }
}