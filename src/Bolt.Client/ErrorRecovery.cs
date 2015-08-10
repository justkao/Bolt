using System;
using System.Net.Http;

namespace Bolt.Client
{
    public class ErrorRecovery : IErrorRecovery
    {
        public bool CanRecover(ClientActionContext context, Exception error)
        {
            if (error is NoServersAvailableException)
            {
                return true;
            }

            if (error is BoltSerializationException)
            {
                throw error;
            }

            var exception = error as BoltServerException;
            if (exception == null)
            {
                return error is HttpRequestException;
            }

            switch (exception.Error)
            {
                case ServerErrorCode.ContractNotFound:
                    return false;
                default:
                    return false;
            }
        }
    }
}