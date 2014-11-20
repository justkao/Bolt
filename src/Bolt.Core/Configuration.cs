namespace Bolt
{
    public class Configuration
    {
        public Configuration(ISerializer serializer)
        {
            Serializer = serializer;
            EndpointProvider = new EndpointProvider();
            SessionHeaderName = "Session-ID";
            ExceptionSerializer = new ExceptionSerializer();
        }

        public Configuration()
        {
            SessionHeaderName = "Session-ID";
            Serializer = new JsonSerializer();
            EndpointProvider = new EndpointProvider();
            ExceptionSerializer = new ExceptionSerializer();
        }

        public ISerializer Serializer { get; set; }

        public IEndpointProvider EndpointProvider { get; set; }

        public string SessionHeaderName { get; set; }

        public IExceptionSerializer ExceptionSerializer { get; set; }
    }
}