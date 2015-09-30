using System;
using Bolt.Metadata;

namespace Bolt.Client
{
    public interface IRequestTimeoutProvider
    {
        TimeSpan GetActionTimeout(Type contract, ActionMetadata actionMetadata);
    }
}