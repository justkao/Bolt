using System;
using System.Threading;

namespace Bolt.Client
{
    public abstract class ContractProxy<TContractDescriptor> : IChannelProvider, ICancellationTokenProvider
        where TContractDescriptor : ContractDescriptor
    {
        protected ContractProxy(ContractProxy<TContractDescriptor> proxy)
        {
            Descriptor = proxy.Descriptor;
            Channel = proxy.Channel;
        }

        protected ContractProxy(TContractDescriptor contractDescriptor, IChannel channel)
        {
            Descriptor = contractDescriptor;
            Channel = channel;
        }

        public TContractDescriptor Descriptor { get; private set; }

        public IChannel Channel { get; set; }

        public virtual ContractProxy<TContractDescriptor> Clone(IChannel channel = null)
        {
            ContractProxy<TContractDescriptor> proxy = (ContractProxy<TContractDescriptor>)Activator.CreateInstance(GetType(), this);
            if (channel != null)
            {
                proxy.Channel = channel;
            }

            return proxy;
        }

        public CancellationToken GetCancellationToken(ActionDescriptor descriptor)
        {
            return Channel.GetCancellationToken(descriptor);
        }
    }
}