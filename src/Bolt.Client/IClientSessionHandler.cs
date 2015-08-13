using System.Net.Http;

namespace Bolt.Client
{
    public interface IClientSessionHandler
    {
        string GetSessionIdentifier(HttpResponseMessage response);

        void EnsureSession(HttpRequestMessage request, string session);
    }
}