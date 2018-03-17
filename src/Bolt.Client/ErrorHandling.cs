using System;
using System.Net.Http;

namespace Bolt.Client
{
    public class ErrorHandling : IErrorHandling
    {
        public virtual ErrorHandlingResult Handle(ClientActionContext context, Exception e)
        {
            if (context.Proxy.State == ProxyState.Closed)
            {
                return ErrorHandlingResult.Close;
            }

            if (e is NoServersAvailableException)
            {
                return ErrorHandlingResult.Recover;
            }

            if ((e as BoltServerException)?.ServerError == ServerErrorCode.ContractNotFound)
            {
                return ErrorHandlingResult.Close;
            }

            if ((e as BoltServerException)?.ServerError == ServerErrorCode.SessionNotFound)
            {
                return ErrorHandlingResult.Recover; 
            }

            if (e is HttpRequestException)
            {
                return ErrorHandlingResult.Recover; 
            }

            if (e is ProxyClosedException)
            {
                return ErrorHandlingResult.Close;
            }

            return ErrorHandlingResult.Rethrow;
        }
    }
}