namespace Bolt.Client
{
    public interface IContractDescriptorProvider<TContractDescriptor> where TContractDescriptor : ContractDescriptor
    {
        TContractDescriptor ContractDescriptor { get; set; }
    }
}