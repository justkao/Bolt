using System;

namespace Bolt.Server
{
    public class HandlerErrorContext
    {
        public ISerializer Serializer { get; set; }

        public IExceptionWrapper ExceptionWrapper { get; set; }

        public BoltServerOptions Options { get; set; }

        public ServerActionContext ActionContext { get; set; }

        public Exception Error { get; set; }

        public ServerErrorCode? ErrorCode { get; set; }
    }
}