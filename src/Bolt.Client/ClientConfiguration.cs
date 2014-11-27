using System;
using System.Linq;
using System.Reflection;
using Bolt.Client.Channels;

namespace Bolt.Client
{
    public class ClientConfiguration : Configuration
    {
        public ClientConfiguration(ISerializer serializer, IExceptionSerializer exceptionSerializer)
            : base(serializer, exceptionSerializer)
        {
            ClientDataHandler = new ClientDataHandler(serializer, ExceptionSerializer);
            RequestForwarder = new RequestForwarder(ClientDataHandler, ServerErrorCodesHeader);
        }

        public IRequestForwarder RequestForwarder { get; set; }

        public IClientDataHandler ClientDataHandler { get; set; }

        public TContract CreateStateLessProxy<TContract, TDescriptor>(Uri uri, TDescriptor descriptor = null)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return CreateStateLessProxy<TContract, TDescriptor>(new UriServerProvider(uri), descriptor);
        }

        public TContract CreateStateFullProxy<TContract, TDescriptor>(Uri uri, TDescriptor descriptor = null)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return CreateStateFullProxy<TContract, TDescriptor>(new UriServerProvider(uri), descriptor);
        }

        public TContract CreateStateFullProxy<TContract, TDescriptor>(IServerProvider serverProvider, TDescriptor descriptor = null)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return
                CreateProxy<TContract, TDescriptor>(
                    new RecoverableStatefullChannel<TContract, TDescriptor>(
                        descriptor ?? CreateDefaultDescriptor<TDescriptor>(),
                        serverProvider,
                        SessionHeader,
                        RequestForwarder,
                        EndpointProvider));
        }

        public TContract CreateStateLessProxy<TContract, TDescriptor>(IServerProvider serverProvider, TDescriptor descriptor = null)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return
                CreateProxy<TContract, TDescriptor>(
                    new RecoverableChannel<TContract, TDescriptor>(
                        descriptor ?? CreateDefaultDescriptor<TDescriptor>(),
                        serverProvider,
                        RequestForwarder,
                        EndpointProvider));
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