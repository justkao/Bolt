using System;
using System.Net.Http;

namespace Bolt.Client
{
    /// <summary>
    /// Describes the Bolt server response with additional metadata attached.
    /// </summary>
    /// <typeparam name="TResponse">The type of response.</typeparam>
    public struct ResponseDescriptor<TResponse>
    {
        public ResponseDescriptor(HttpResponseMessage response, ClientActionContext context, Exception error, ResponseError errorType)
            : this()
        {
            Context = context;
            ErrorType = errorType;
            Error = error;
            Response = response;
        }

        public ResponseDescriptor(HttpResponseMessage response, ClientActionContext context, TResponse result)
            : this()
        {
            Response = response;
            Result = result;
            Context = context;
            ErrorType = ResponseError.None;
            Error = null;
        }

        /// <summary>
        /// The server response.
        /// </summary>
        public HttpResponseMessage Response { get; private set; }

        /// <summary>
        /// The context of request action.
        /// </summary>
        public ClientActionContext Context { get; private set; }

        /// <summary>
        /// Error that occurred durring processing of action.
        /// </summary>
        public Exception Error { get; }

        /// <summary>
        /// The type of error that occurred.
        /// </summary>
        public ResponseError ErrorType { get; }

        /// <summary>
        /// Determines whether request was successful.
        /// </summary>
        public bool IsSuccess => ErrorType == ResponseError.None;

        /// <summary>
        /// Gets the deserialized server result. 
        /// </summary>
        /// <remarks>
        /// <see cref="Empty.Instance"/> is returned if client do not expect any data.
        /// </remarks>
        public TResponse Result { get; }

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