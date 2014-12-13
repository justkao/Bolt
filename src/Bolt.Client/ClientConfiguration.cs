using System;

namespace Bolt.Client
{
    public class ClientConfiguration : Configuration
    {
        public ClientConfiguration(IExceptionSerializer exceptionSerializer, IWebRequestHandler webRequestHandler = null)
            : base(new XmlSerializer(), exceptionSerializer)
        {
            if (webRequestHandler == null)
            {
                webRequestHandler = new DefaultWebRequestHandler();
            }

            DataHandler = new DataHandler(Serializer, ExceptionSerializer, webRequestHandler);
            RequestForwarder = new RequestForwarder(DataHandler, webRequestHandler, new ServerErrorProvider(ServerErrorCodesHeader));
        }

        public ClientConfiguration(ISerializer serializer, IExceptionSerializer exceptionSerializer, IWebRequestHandler webRequestHandler = null)
            : base(serializer, exceptionSerializer)
        {
            if (webRequestHandler == null)
            {
                webRequestHandler = new DefaultWebRequestHandler();
            }

            DataHandler = new DataHandler(serializer, ExceptionSerializer, webRequestHandler);
            RequestForwarder = new RequestForwarder(DataHandler, webRequestHandler, new ServerErrorProvider(ServerErrorCodesHeader));
        }

        public IRequestForwarder RequestForwarder { get; set; }

        public IDataHandler DataHandler { get; set; }

        public TimeSpan DefaultResponseTimeout { get; set; }
    }
}