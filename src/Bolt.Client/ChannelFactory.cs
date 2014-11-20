using System;

namespace Bolt.Client
{
    public class ChannelFactory<TChannel, TContractDescriptor>
        where TChannel : Channel, IContractDescriptorProvider<TContractDescriptor>, new()
        where TContractDescriptor : ContractDescriptor
    {
        public ClientConfiguration ClientConfiguration { get; set; }

        public string Prefix { get; set; }

        public ContractDefinition ContractDefinition { get; set; }

        public TContractDescriptor ContractDescriptor { get; set; }

        public virtual TChannel Create(Uri server)
        {
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