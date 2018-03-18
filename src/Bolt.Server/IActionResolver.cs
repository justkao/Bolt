using Bolt.Metadata;
using System;

namespace Bolt.Server
{
    public interface IActionResolver
    {
        ActionMetadata Resolve(ContractMetadata contract, ReadOnlySpan<char> actionName);
    }
}
