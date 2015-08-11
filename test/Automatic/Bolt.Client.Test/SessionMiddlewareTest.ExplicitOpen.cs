using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

using Bolt.Client.Pipeline;
using Bolt.Pipeline;
using Bolt.Session;

using Moq;

using Xunit;

namespace Bolt.Client.Test
{
    public abstract partial class SessionMiddlewareTest
    {
        public class ExplicitOpen : SessionMiddlewareTest
        {
            [Fact]
            public void New_EnsureUnititialized()
            {
                var pipeline = CreatePipeline();
                var proxy = CreateProxy(pipeline);

                Assert.Equal(ProxyState.Uninitialized, proxy.State);
            }

            [Fact]
            public async Task Open_NoSessionIdReceived_Throws()
            {
                var pipeline = CreatePipeline();
                SessionHandler.Setup(s => s.GetSessionIdentifier(It.IsAny<HttpResponseMessage>())).Returns((string)null).Verifiable();
                SessionErrorHandling.Setup(r => r.Handle(It.IsAny<ClientActionContext>(), It.IsAny<Exception>())).Returns(SessionHandlingResult.Recover);

                var proxy = CreateProxy(pipeline);

                await Assert.ThrowsAsync<BoltServerException>(() => proxy.OpenAsync());

                SessionHandler.Verify();
            }

            [Fact]
            public async Task OpenExplicit_Ok()
            {
                string sessionid = "temp session";
                InitSessionResult result = new InitSessionResult();

                var pipeline = CreatePipeline(
                    (next, ctxt) =>
                        {
                            Assert.Equal(BoltFramework.InitSessionAction, ctxt.Action);
                            ctxt.ActionResult = result;
                            ctxt.Connection = DefaultDescriptor;
                            return next(ctxt);
                        });

                SetupSessionHandler(sessionid);

                var proxy = CreateProxy(pipeline);

                await proxy.OpenAsync();

                Assert.Equal(DefaultDescriptor, pipeline.Find<SessionMiddleware>().Connection);
                Assert.Equal(result, pipeline.Find<SessionMiddleware>().InitSessionResult);
                Assert.Equal(ProxyState.Open, ProxyState.Open);
                SessionHandler.Verify();
            }

            [Fact]
            public async Task Open_EnsureConnection()
            {
                string sessionid = "temp session";
                InitSessionResult result = new InitSessionResult();
                ConnectionDescriptor descriptor = new ConnectionDescriptor(new Uri("http://localhost"));

                var pipeline = CreatePipeline(
                    (next, ctxt) =>
                        {
                            ctxt.Connection = descriptor;
                            ctxt.ActionResult = result;
                            return next(ctxt);
                        });

                SetupSessionHandler(sessionid);

                var proxy = CreateProxy(pipeline);
                await proxy.OpenAsync();

                Assert.Equal(result, pipeline.Find<SessionMiddleware>().InitSessionResult);
                Assert.Equal(ProxyState.Open, ProxyState.Open);
                SessionHandler.Verify();
            }

            [InlineData(true)]
            [InlineData(false)]
            [Theory]
            public async Task OpenExplicit_Throws_EnsureProperState(bool canRecover)
            {
                string sessionid = "temp session";
                SessionErrorHandling.Setup(e => e.Handle(It.IsAny<ClientActionContext>(), It.IsAny<InvalidOperationException>()))
                    .Returns(canRecover ? SessionHandlingResult.Recover : SessionHandlingResult.Close);

                var pipeline = CreatePipeline(
                    (next, ctxt) =>
                        {
                            throw new InvalidOperationException();
                        });

                SetupSessionHandler(sessionid);

                var proxy = CreateProxy(pipeline);
                await Assert.ThrowsAsync<InvalidOperationException>(() => proxy.OpenAsync());

                if (canRecover)
                {
                    Assert.Equal(ProxyState.Uninitialized, proxy.State);
                }
                else
                {
                    Assert.Equal(ProxyState.Closed, proxy.State);
                }

                SessionErrorHandling.Verify();
            }

            [Fact]
            public async Task Execute_OpenedSession_EnsureSessionId()
            {
                string sessionid = "temp session";
                IPipeline<ClientActionContext> pipeline = CreatePipeline(
                    (next, ctxt) =>
                        {
                            ctxt.Connection = DefaultDescriptor;
                            return next(ctxt);
                        });

                SetupSessionHandler(sessionid);
                SessionHandler.Setup(s => s.EnsureSession(It.IsAny<HttpRequestMessage>(), sessionid)).Verifiable();

                var proxy = CreateProxy(pipeline);
                await proxy.OpenAsync();
                await proxy.Execute();

                SessionHandler.Verify();
            }

            [Fact]
            public async Task Close_Uninitialized_Ok()
            {
                IPipeline<ClientActionContext> pipeline = CreatePipeline();
                var proxy = CreateProxy(pipeline);
                await proxy.CloseAsync();

                Assert.Equal(ProxyState.Closed, proxy.State);
            }
        }
    }
}
