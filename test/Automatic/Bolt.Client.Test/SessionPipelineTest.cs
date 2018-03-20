using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client.Pipeline;
using Xunit;

namespace Bolt.Client.Test
{
    public class SessionPipelineTest : HttpMessageHandler
    {
        public SessionPipelineTest()
        {
            Configuration = new ClientConfiguration();

            Proxy = Configuration.ProxyBuilder()
                    .Recoverable(1, TimeSpan.FromMilliseconds(1))
                    .UseSession(false)
                    .Url("http://dummy.tmp")
                    .UseHttpMessageHandler(this).Build<ITestContract>();
        }

        public ClientConfiguration Configuration { get; set; }

        public ITestContract Proxy { get; set; }

        public Func<HttpResponseMessage> SendAction { get; set; }

        [Fact]
        public void OpenSession_Ok()
        {
            SendAction = () =>
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Headers.Add(Configuration.Options.SessionHeader, "dummy");
                return response;
            };

            Proxy.OpenSession();
            Assert.Equal(ProxyState.Open, ((ProxyBase)Proxy).State);
            Assert.NotNull(((ProxyBase)Proxy).Pipeline.Find<SessionMiddleware>().GetSession(Proxy));
        }

        [Fact]
        public void DisposeProxy_ServerDown_EnsureProxyClosed()
        {
            SendAction = () =>
            {
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Headers.Add(Configuration.Options.SessionHeader, "dummy");
                return response;
            };

            Proxy.OpenSession();

            SendAction = () =>
            {
                throw new HttpRequestException("Dummy");
            };

            Assert.Throws<HttpRequestException>(() => ((IDisposable)Proxy).Dispose());
            Assert.Equal(ProxyState.Closed, ((IProxy)Proxy).State);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(SendAction());
        }

        public interface ITestContract
        {
            [InitSession]
            Task OpenSession();

            [DestroySession]
            Task CloseSession();

            void Execute();
        }
    }
}
