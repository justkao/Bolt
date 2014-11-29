using System;

namespace Bolt.Client
{
    public class ServerErrorProvider : IServerErrorProvider
    {
        private readonly string _errorCodeHeader;

        public ServerErrorProvider(string errorCodeHeader)
        {
            if (errorCodeHeader == null)
            {
                throw new ArgumentNullException("errorCodeHeader");
            }

            _errorCodeHeader = errorCodeHeader;
        }

        public ServerErrorCode? TryRead(ClientActionContext context)
        {
            string value = context.Response.Headers[_errorCodeHeader];
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

        public int? TryReadErrorCode(ClientActionContext context)
        {
            string value = context.Response.Headers[_errorCodeHeader];
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