namespace Bolt.Client
{
    public class ClientConfiguration : Configuration
    {
        public ClientConfiguration()
        {
            ClientDataHandler = new ClientDataHandler(Serializer);
            RequestForwarder = new RequestForwarder(ClientDataHandler);
        }

        public virtual void Update(Channel channel)
        {
            channel.DataHandler = ClientDataHandler;
            channel.RequestForwarder = RequestForwarder;

            if (channel is StatefullChannel)
            {
                (channel as StatefullChannel).SessionHeader = SessionHeaderName;
            }
        }

        public IRequestForwarder RequestForwarder { get; set; }

        public IClientDataHandler ClientDataHandler { get; set; }
    }
}