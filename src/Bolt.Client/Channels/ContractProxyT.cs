namespace Bolt.Client.Channels
{
    public abstract class ContractProxy<TContractDescriptor> : ContractProxy
        where TContractDescriptor : ContractDescriptor
    {
        protected ContractProxy(ContractProxy proxy)
            : base(proxy)
        {
        }

        protected ContractProxy(IChannel channel)
            : base(ContractDescriptor<TContractDescriptor>.Instance, channel)
        {
        }

        public new TContractDescriptor Descriptor
        {
            get { return (TContractDescriptor)base.Descriptor; }
            set { base.Descriptor = value; }
        }
    }
}
