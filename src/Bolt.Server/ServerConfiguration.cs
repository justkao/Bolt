namespace Bolt.Server
{
    public class ServerConfiguration : Configuration
    {
        public ServerConfiguration(ISerializer serializer, IExceptionSerializer exceptionSerializer)
            : base(serializer, exceptionSerializer)
        {
            ServerDataHandler = new ServerDataHandler(serializer, ExceptionSerializer);
            ResponseHandler = new ResponseHandler(ServerDataHandler);
        }


        public IResponseHandler ResponseHandler { get; set; }

        public IServerDataHandler ServerDataHandler { get; set; }
    }
}
