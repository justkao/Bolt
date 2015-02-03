using System;
using System.Net;

namespace Bolt.Client
{
    /// <summary>
    /// Describes the Bolt server response with additional metadata attached.
    /// </summary>
    /// <typeparam name="TResponse">The type of response.</typeparam>
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

        /// <summary>
        /// The server response.
        /// </summary>
        public HttpWebResponse Response { get; private set; }

        /// <summary>
        /// The context of request action.
        /// </summary>
        public ClientActionContext Context { get; private set; }

        /// <summary>
        /// Error that occurred durring processing of action.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// The type of error that occurred.
        /// </summary>
        public ResponseErrorType ErrorType { get; private set; }

        /// <summary>
        /// Determines whether request was successful.
        /// </summary>
        public bool IsSuccess
        {
            get { return ErrorType == ResponseErrorType.None; }
        }

        /// <summary>
        /// Gets the deserialized server result. 
        /// </summary>
        /// <remarks>
        /// <see cref="Empty.Instance"/> is returned if client do not expect any data.
        /// </remarks>
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