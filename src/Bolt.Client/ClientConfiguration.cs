using System;
using System.Linq;
using System.Reflection;

namespace Bolt.Client
{
    public class ClientConfiguration : Configuration
    {
        public ClientConfiguration(ISerializer serializer, IExceptionSerializer exceptionSerializer)
            : base(serializer, exceptionSerializer)
        {
            ClientDataHandler = new ClientDataHandler(serializer, ExceptionSerializer);
            RequestForwarder = new RequestForwarder(ClientDataHandler);
        }

        public IRequestForwarder RequestForwarder { get; set; }

        public IClientDataHandler ClientDataHandler { get; set; }

        public TContract CreateStateLessProxy<TContract, TDescriptor>(Uri uri, string prefix, TDescriptor descriptor = null)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return CreateStateLessProxy<TContract, TDescriptor>(new UriServerProvider(uri), prefix, descriptor);
        }

        public TContract CreateStateFullProxy<TContract, TDescriptor>(Uri uri, string prefix, TDescriptor descriptor = null)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return CreateStateFullProxy<TContract, TDescriptor>(new UriServerProvider(uri), prefix, descriptor);
        }

        public TContract CreateStateFullProxy<TContract, TDescriptor>(IServerProvider serverProvider, string prefix, TDescriptor descriptor = null)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return
                CreateProxy<TContract, TDescriptor>(
                    new StateFullChannel<TContract, TDescriptor>(descriptor ?? CreateDefaultDescriptor<TDescriptor>(),
                        serverProvider, prefix, (c) => (TContract)Activator.CreateInstance(typeof(TContract), c), RequestForwarder, EndpointProvider));
        }

        public TContract CreateStateLessProxy<TContract, TDescriptor>(IServerProvider serverProvider, string prefix, TDescriptor descriptor = null)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return CreateProxy<TContract, TDescriptor>(new StateLessChannel(descriptor ?? CreateDefaultDescriptor<TDescriptor>(), serverProvider, prefix, RequestForwarder, EndpointProvider));
        }

        public TContract CreateProxy<TContract, TDescriptor>(IChannel channel)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return (TContract)Activator.CreateInstance(typeof(TContract), channel);
        }

        public virtual TDescriptor CreateDefaultDescriptor<TDescriptor>()
        {
            FieldInfo defaultValue = typeof(TDescriptor).GetTypeInfo().DeclaredFields.First(m => m.IsStatic && m.Name == "Default");
            return (TDescriptor)defaultValue.GetValue(null);
        }
    }
}