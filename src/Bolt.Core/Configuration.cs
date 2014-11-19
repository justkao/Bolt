namespace Bolt
{
    public class Configuration
    {
        public Configuration()
        {
            SessionHeaderName = "Session-ID";
            Serializer = new ProtocolBufferSerializer();
            EndpointProvider = new EndpointProvider();
        }

        public ISerializer Serializer { get; set; }

        public IEndpointProvider EndpointProvider { get; set; }

        public string SessionHeaderName { get; set; }
    }
}