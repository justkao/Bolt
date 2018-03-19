using System;
using Bolt.Metadata;

namespace Bolt.Server
{
    public interface IActionResolver
    {
        ActionMetadata Resolve(ContractMetadata contract, ReadOnlySpan<char> actionName);
    }
}
