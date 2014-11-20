namespace Bolt.Client
{
    public class ClientConfiguration : Configuration
    {
        public ClientConfiguration(ISerializer serializer)
            : base(serializer)
        {
            ExceptionSerializer = new ExceptionSerializer();
            ClientDataHandler = new ClientDataHandler(serializer, ExceptionSerializer);
            RequestForwarder = new RequestForwarder(ClientDataHandler);
        }

        public ClientConfiguration()
        {
            ExceptionSerializer = new ExceptionSerializer();
            ClientDataHandler = new ClientDataHandler(Serializer, ExceptionSerializer);
            RequestForwarder = new RequestForwarder(ClientDataHandler);
        }

        public virtual void Update(Channel channel)
        {
            channel.DataHandler = ClientDataHandler;
            channel.RequestForwarder = RequestForwarder;
            channel.EndpointProvider = EndpointProvider;

            if (channel is StatefullChannel)
            {
                (channel as StatefullChannel).SessionHeader = SessionHeaderName;
            }
        }

        public IRequestForwarder RequestForwarder { get; set; }

        public IClientDataHandler ClientDataHandler { get; set; }

        public IExceptionSerializer ExceptionSerializer { get; set; }
    }
}