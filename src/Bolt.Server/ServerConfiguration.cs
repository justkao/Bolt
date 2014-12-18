using System;

namespace Bolt.Server
{
    public class ServerConfiguration : Configuration
    {
        public ServerConfiguration()
        {
            DataHandler = new DataHandler(Serializer, ExceptionSerializer);
            ErrorHandler = new ErrorHandler(DataHandler, ServerErrorCodesHeader);
            ResponseHandler = new ResponseHandler(DataHandler);
        }

        public ServerConfiguration(ISerializer serializer, IExceptionSerializer exceptionSerializer)
            : base(serializer, exceptionSerializer)
        {
            DataHandler = new DataHandler(serializer, ExceptionSerializer);
            ErrorHandler = new ErrorHandler(DataHandler, ServerErrorCodesHeader);
            ResponseHandler = new ResponseHandler(DataHandler);
        }

        public IResponseHandler ResponseHandler { get; set; }

        public IDataHandler DataHandler { get; set; }

        public TimeSpan StateFullInstanceLifetime { get; set; }

        public IErrorHandler ErrorHandler { get; set; }
    }
}
