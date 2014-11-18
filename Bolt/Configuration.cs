namespace Bolt
{
    public class Configuration
    {
        public Configuration()
        {
            SessionHeaderName = "Session-ID";
            Serializer = new ProtocolBufferSerializer();
        }

        public ISerializer Serializer { get; set; }

        public string SessionHeaderName { get; set; }
    }
}