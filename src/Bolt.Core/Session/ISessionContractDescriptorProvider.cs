using System;

namespace Bolt.Session
{
    public interface ISessionContractDescriptorProvider
    {
        SessionContractDescriptor Resolve(Type contract);
    }
}