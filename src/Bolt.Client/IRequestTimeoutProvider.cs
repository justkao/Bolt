using System;
using Bolt.Metadata;

namespace Bolt.Client
{
    public interface IRequestTimeoutProvider
    {
        TimeSpan GetActionTimeout(ContractMetadata contract, ActionMetadata actionMetadata);
    }
}