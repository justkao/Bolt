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

        public virtual void Update(Channel channel)
        {
            channel.DataHandler = ClientDataHandler;
            channel.RequestForwarder = RequestForwarder;
            channel.EndpointProvider = EndpointProvider;
            channel.SessionHeader = SessionHeaderName;
            channel.SessionHeader = SessionHeaderName;
        }

        public IRequestForwarder RequestForwarder { get; set; }

        public IClientDataHandler ClientDataHandler { get; set; }
    }
}