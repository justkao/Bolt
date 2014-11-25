using System;
using System.Threading;

namespace Bolt.Client.Channels
{
    public abstract class ContractProxy<TContractDescriptor> : ICancellationTokenProvider
        where TContractDescriptor : ContractDescriptor
    {
        protected ContractProxy(ContractProxy<TContractDescriptor> proxy)
        {
            if (proxy == null)
            {
                throw new ArgumentNullException("proxy");
            }

            Descriptor = proxy.Descriptor;
            Channel = proxy.Channel;
        }

        protected ContractProxy(TContractDescriptor contractDescriptor, IChannel channel)
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

        public TContractDescriptor Descriptor { get; private set; }

        public IChannel Channel { get; private set; }

        public virtual ContractProxy<TContractDescriptor> Clone(IChannel channel = null)
        {
            ContractProxy<TContractDescriptor> proxy = (ContractProxy<TContractDescriptor>)Activator.CreateInstance(GetType(), this);
            if (channel != null)
            {
                proxy.Channel = channel;
            }

            return proxy;
        }

        public virtual CancellationToken GetCancellationToken(ActionDescriptor descriptor)
        {
            return Channel.GetCancellationToken(descriptor);
        }
    }
}