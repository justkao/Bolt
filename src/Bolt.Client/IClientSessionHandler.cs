using System;
using System.Net.Http;

namespace Bolt.Client.Channels
{
    public interface IClientSessionHandler
    {
        string GetSessionIdentifier(HttpResponseMessage response);

        void EnsureSession(HttpRequestMessage request, string session);
    }
}