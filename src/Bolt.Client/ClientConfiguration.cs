using Bolt.Client.Channels;
using System;

namespace Bolt.Client
{
    public class ClientConfiguration : Configuration
    {
        public ClientConfiguration(ISerializer serializer, IExceptionSerializer exceptionSerializer, IWebRequestHandler webRequestHandler = null)
            : base(serializer, exceptionSerializer)
        {
            ClientDataHandler = new ClientDataHandler(serializer, ExceptionSerializer);
            RequestForwarder = new RequestForwarder(ClientDataHandler, webRequestHandler, ServerErrorCodesHeader);
        }

        public IRequestForwarder RequestForwarder { get; set; }

        public IClientDataHandler ClientDataHandler { get; set; }

        public TimeSpan DefaultResponseTimeout { get; set; }

        public TContract CreateProxy<TContract, TDescriptor>(Uri uri, TDescriptor descriptor = null)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return CreateProxy<TContract, TDescriptor>(new UriServerProvider(uri), descriptor);
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
            return CreateProxy<TContract, TDescriptor>(this.CreateStateFullRecoverable<TContract, TDescriptor>(serverProvider, descriptor));
        }

        public TContract CreateProxy<TContract, TDescriptor>(IServerProvider serverProvider, TDescriptor descriptor = null)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return CreateProxy<TContract, TDescriptor>(this.CreateRecoverable<TContract, TDescriptor>(serverProvider, descriptor));
        }

        public TContract CreateProxy<TContract, TDescriptor>(IChannel channel)
            where TContract : ContractProxy<TDescriptor>
            where TDescriptor : ContractDescriptor
        {
            return (TContract)Activator.CreateInstance(typeof(TContract), channel);
        }
    }
}