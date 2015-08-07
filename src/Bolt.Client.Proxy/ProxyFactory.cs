using System;

using Bolt.Client.Channels;

using Castle.DynamicProxy;

namespace Bolt.Client.Proxy
{
    public class ProxyFactory
    {
        private readonly ProxyGenerator _generator = new ProxyGenerator();

        public TContract CreateProxy<TContract>(IChannel channel) where TContract:class
        {
            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            Bolt.ValidateContract(typeof(TContract));
            var interceptor = new ChannelInterceptor(typeof(TContract), channel);
            var options = new ProxyGenerationOptions() { BaseTypeForInterfaceProxy = typeof(ContractProxy) };

            TContract proxy = _generator.CreateInterfaceProxyWithoutTarget<TContract>(options, interceptor);
            ((DynamicContractProxy)(object)proxy).Initialize(typeof(TContract), channel);
            return proxy;
        }

        public class DynamicContractProxy : ContractProxy
        {
            internal void Initialize(Type contract, IChannel channel)
            {
                Channel = channel;
                Contract = contract;
            }
        }

        private class ChannelInterceptor : IInterceptor
        {
            private readonly Type _contract;

            private readonly IChannel _channel;

            public ChannelInterceptor(Type contract, IChannel channel)
            {
                _contract = contract;
                _channel = channel;
            }

            public void Intercept(IInvocation invocation)
            {
                throw new NotImplementedException();
            }
        }
    }
}
