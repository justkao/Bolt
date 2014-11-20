using System;

namespace Bolt.Client
{
    public class ChannelFactory<TChannel, TContractDescriptor>
        where TChannel : Channel, IContractDescriptorProvider<TContractDescriptor>, new()
        where TContractDescriptor : ContractDescriptor
    {
        public ChannelFactory(ContractDefinition contractDefinition, TContractDescriptor contractDescriptor)
        {
            if (contractDefinition == null)
            {
                throw new ArgumentNullException("contractDefinition");
            }

            if (contractDescriptor == null)
            {
                throw new ArgumentNullException("contractDescriptor");
            }

            ContractDefinition = contractDefinition;
            ContractDescriptor = contractDescriptor;
        }

        public ClientConfiguration ClientConfiguration { get; set; }

        public string Prefix { get; set; }

        public ContractDefinition ContractDefinition { get; private set; }

        public TContractDescriptor ContractDescriptor { get; private set; }

        public virtual TChannel Create(Uri server = null)
        {
            if (ClientConfiguration == null)
            {
                throw new InvalidOperationException("ClientConfiguration not initialized.");
            }

            TChannel channel = new TChannel();
            ClientConfiguration.Update(channel);
            channel.Contract = ContractDefinition;
            channel.Prefix = Prefix;
            channel.ContractDescriptor = ContractDescriptor;
            channel.ServerUrl = server;
            return channel;
        }
    }
}