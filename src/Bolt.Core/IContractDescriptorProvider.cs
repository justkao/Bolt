
namespace Bolt
{
    public interface IContractDescriptorProvider<T> where T : ContractDescriptor
    {
        T Descriptor { get; }
    }
}
