
namespace Bolt
{
    /// <summary>
    /// Indicates that object can provide instance of <see cref="ContractDescriptor"/>
    /// </summary>
    public interface IContractDescriptorProvider
    {
        /// <summary>
        /// Gets the contract descriptor assigned to object.
        /// </summary>
        ContractDescriptor Descriptor { get; }
    }
}
