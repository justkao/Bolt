using System;
using System.Net.Http;

namespace Bolt.Client
{
    public class ErrorHandling : IErrorHandling
    {
        public SessionHandlingResult Handle(ClientActionContext context, Exception e)
        {
            if (e is NoServersAvailableException)
            {
                return SessionHandlingResult.Recover;
            }

            if (e is BoltSerializationException)
            {
                return SessionHandlingResult.Rethrow;
            }

            if ((e as BoltServerException)?.Error == ServerErrorCode.ContractNotFound)
            {
                return SessionHandlingResult.Close;
            }

            if ((e as BoltServerException)?.Error == ServerErrorCode.SessionNotFound)
            {
                return SessionHandlingResult.Recover; 
            }

            if (e is HttpRequestException)
            {
                return SessionHandlingResult.Recover; 
            }

            if (e is ProxyClosedException)
            {
                return SessionHandlingResult.Close;
            }

            return SessionHandlingResult.Rethrow;
        }
    }
}