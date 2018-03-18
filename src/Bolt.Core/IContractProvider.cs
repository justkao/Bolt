using Bolt.Metadata;
using System;

namespace Bolt
{
    /// <summary>
    /// Indicates that object can provide contract type.
    /// </summary>
    public interface IContractProvider
    {
        /// <summary>
        /// Gets the contract type assigned to object.
        /// </summary>
        ContractMetadata Contract { get; }
    }
}
