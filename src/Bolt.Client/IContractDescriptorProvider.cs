namespace Bolt.Client
{
    public interface IContractDescriptorProvider<TContractDescriptor> where TContractDescriptor : ContractDescriptor
    {
        TContractDescriptor Descriptor { get; set; }
    }
}