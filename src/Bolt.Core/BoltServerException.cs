using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Bolt
{
    /// <summary>
    /// Exception indicating that special Bolt error occurred on server.
    /// </summary>
    [Serializable]
    public class BoltServerException : BoltException
    {
        public BoltServerException(string message, ServerErrorCode errorCode)
            : base(message)
        {
            ServerError = errorCode;
        }

        public BoltServerException(string message, ServerErrorCode errorCode, Exception innerException)
            : base(message, innerException)
        {
            ServerError = errorCode;
        }

        public BoltServerException(int errorCode, string action, string url)
            : base($"Execution of action '{action}' failed on server with error code '{errorCode}'. Url - '{url}'")
        {
            ErrorCode = errorCode;
            Action = action;
        }

        public BoltServerException(ServerErrorCode error, string action, string url, Exception innerException)
            : base($"Execution of action '{action}' failed on server with error '{error}'. Url - '{url}'", innerException)
        {
            ServerError = error;
            Action = action;
            Url = url;
        }

        public BoltServerException(string message, ServerErrorCode error, string action, string url, Exception innerException)
            : base(message, innerException)
        {
            ServerError = error;
            Action = action;
            Url = url;
        }

        public BoltServerException(string message, ServerErrorCode error, string action, string url)
            : base(message)
        {
            ServerError = error;
            Action = action;
            Url = url;
        }

        public BoltServerException(ServerErrorCode error, string action, string url)
            : base($"Execution of action '{action}' failed on server with error '{error}'. Url - '{url}'")
        {
            ServerError = error;
            Action = action;
            Url = url;
        }

        public BoltServerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            var serverError = info.GetInt32(nameof(ServerError));
            ServerError = serverError != -1 ? (ServerErrorCode)serverError : (ServerErrorCode?)null;
            ErrorCode = info.GetInt32(nameof(ErrorCode));
            Action = info.GetString(nameof(Action));
            Url = info.GetString(nameof(Url));
        }

        public ServerErrorCode? ServerError { get; }

        public int? ErrorCode { get; }

        public string Action { get; }

        public string Url { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ServerError), ServerError != null ? (int)ServerError : -1);
            info.AddValue(nameof(ErrorCode), ErrorCode);
            info.AddValue(nameof(Action), Action);
            info.AddValue(nameof(Url), Url);

            base.GetObjectData(info, context);
        }
    }
}
