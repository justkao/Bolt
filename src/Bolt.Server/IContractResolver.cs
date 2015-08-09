using System;
using System.Collections.Generic;

namespace Bolt.Server
{
    public interface IContractResolver
    {
        Type Resolve(IEnumerable<Type> contracts, string contractName);
    }
}