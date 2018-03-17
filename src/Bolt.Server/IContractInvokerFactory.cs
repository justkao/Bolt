using System;

namespace Bolt.Server
{
    public interface IContractInvokerFactory
    {
        IContractInvoker Create(Type contract, IInstanceProvider instanceProvider, ServerRuntimeConfiguration configuration);
    }
}