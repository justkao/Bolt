using System;
using System.Net;

namespace Bolt.Client
{
    public struct ResponseDescriptor<TResponse>
    {
        public ResponseDescriptor(HttpWebResponse response, ClientActionContext context, Exception error, ResponseErrorType errorType)
            : this()
        {
            Context = context;
            ErrorType = errorType;
            Error = error;
            Response = response;
        }

        public ResponseDescriptor(HttpWebResponse response, ClientActionContext context, TResponse result)
            : this()
        {
            Response = response;
            Result = result;
            Context = context;
            ErrorType = ResponseErrorType.None;
            Error = null;
        }

        public HttpWebResponse Response { get; private set; }

        public ClientActionContext Context { get; private set; }

        public Exception Error { get; private set; }

        public ResponseErrorType ErrorType { get; private set; }

        public bool IsSuccess
        {
            get { return ErrorType == ResponseErrorType.None; }
        }

        public TResponse Result { get; private set; }

        public TResponse GetResultOrThrow()
        {
            if (!IsSuccess)
            {
                throw Error;
            }

            return Result;
        }
    }
}