using System;
using System.Net;

namespace Bolt.Client
{
    public class ClientErrorProvider : IClientErrorProvider
    {
        private readonly string _errorCodeHeader;

        public ClientErrorProvider(string errorCodeHeader)
        {
            if (errorCodeHeader == null)
            {
                throw new ArgumentNullException(nameof(errorCodeHeader));
            }

            _errorCodeHeader = errorCodeHeader;
        }

        public virtual BoltServerException TryReadServerError(ClientActionContext context)
        {
            ServerErrorCode? result = TryReadBoltError(context);
            if (result != null)
            {
                return new BoltServerException(result.Value, context.Action, context.Request.RequestUri.ToString());
            }

            int? code = TryReadErrorCode(context);
            if (code != null)
            {
                return new BoltServerException(code.Value, context.Action, context.Request.RequestUri.ToString());
            }

            if (context.Response?.StatusCode == HttpStatusCode.NotFound)
            {
                return new BoltServerException(
                    ServerErrorCode.BoltUnavailable,
                    context.Action,
                    context.Request.RequestUri.ToString());
            }

            return null;
        }

        protected virtual ServerErrorCode? TryReadBoltError(ClientActionContext context)
        {
            string value = context.Response.Headers.GetHeaderValue(_errorCodeHeader);
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            ServerErrorCode code;
            if (Enum.TryParse(value, true, out code))
            {
                return code;
            }

            return null;
        }

        protected virtual int? TryReadErrorCode(ClientActionContext context)
        {
            string value = context.Response.Headers.GetHeaderValue(_errorCodeHeader);
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            int code;
            if (int.TryParse(value, out code))
            {
                return code;
            }

            return null;
        }
    }
}