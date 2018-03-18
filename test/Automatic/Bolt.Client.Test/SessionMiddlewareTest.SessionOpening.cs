using System;
using System.Net.Http;
using System.Threading.Tasks;
using Bolt.Client.Pipeline;
using Moq;
using Xunit;

namespace Bolt.Client.Test
{
    public abstract partial class SessionMiddlewareTest
    {
        public class SessionOpening : SessionMiddlewareTest
        {
            [Fact]
            public void EnsureReady()
            {
                var pipeline = CreatePipeline();
                var proxy = CreateProxy(pipeline);

                Assert.Equal(ProxyState.Ready, proxy.State);
            }

            [Fact]
            public async Task OpenUsingIProxy_RequiredParameters_Throws()
            {
                var pipeline = CreatePipeline();
                SessionHandler.Setup(s => s.GetSessionIdentifier(It.IsAny<HttpResponseMessage>())).Returns((string)null).Verifiable();
                SessionErrorHandling.Setup(r => r.Handle(It.IsAny<ClientActionContext>(), It.IsAny<Exception>())).Returns(ErrorHandlingResult.Recover);

                SetupSessionHandler("test session");

                var proxy = CreateProxy(pipeline);
                await Assert.ThrowsAsync<BoltClientException>(() => proxy.OpenAsync());

                SessionHandler.Verify();
            }

            [Fact]
            public async Task Open_NoSessionIdReceived_Throws()
            {
                var pipeline = CreatePipeline();
                SessionHandler.Setup(s => s.GetSessionIdentifier(It.IsAny<HttpResponseMessage>())).Returns((string)null).Verifiable();
                SessionErrorHandling.Setup(r => r.Handle(It.IsAny<ClientActionContext>(), It.IsAny<Exception>())).Returns(ErrorHandlingResult.Recover);

                var proxy = CreateProxy(pipeline);
                await Assert.ThrowsAsync<BoltClientException>(() => proxy.OpenSession("test"));

                SessionHandler.Verify();
            }

            [Fact]
            public async Task Open_Ok()
            {
                string sessionid = "temp session";
                var pipeline = CreatePipeline(
                    (next, ctxt) =>
                        {
                            Assert.Equal(SessionContract.InitSession.Action, ctxt.Action.Action);
                            Assert.Equal("test", ctxt.Parameters[0]);
                            ctxt.ActionResult = "result";
                            ctxt.ServerConnection = ConnectionDescriptor;
                            return next(ctxt);
                        });
                SetupSessionHandler(sessionid);

                var proxy = CreateProxy(pipeline);
                var result = await proxy.OpenSession("test");

                SessionMetadata session = pipeline.Find<SessionMiddleware>().GetSession(proxy);
                Assert.Equal(ConnectionDescriptor, session.ServerConnection);
                Assert.Equal(result, session.InitSessionResult);
                Assert.Equal(ProxyState.Open, ProxyState.Open);
                SessionHandler.Verify();
            }

            [Fact]
            public async Task Open_Twice_Ok()
            {
                int calls = 0;

                string sessionid = "temp session";
                var pipeline = CreatePipeline(
                    (next, ctxt) =>
                        {
                            calls++;
                            ctxt.ActionResult = "result";
                            ctxt.ServerConnection = ConnectionDescriptor;
                            return next(ctxt);
                        });

                SetupSessionHandler(sessionid);

                var proxy = CreateProxy(pipeline);
                var result = await proxy.OpenSession("test");
                Assert.Equal(result, await proxy.OpenSession("test"));

                SessionHandler.Verify();

                Assert.Equal(1, calls);
            }

            [Fact]
            public async Task Open_EnsureConnection()
            {
                string sessionid = "temp session";
                ConnectionDescriptor descriptor = new ConnectionDescriptor(new Uri("http://localhost"));

                var pipeline = CreatePipeline(
                    (next, ctxt) =>
                        {
                            ctxt.ServerConnection = descriptor;
                            ctxt.ActionResult = "result value";
                            return next(ctxt);
                        });

                SetupSessionHandler(sessionid);

                var proxy = CreateProxy(pipeline);
                var openResult = await proxy.OpenSession("param");

                Assert.Equal(ProxyState.Open, ProxyState.Open);
                Assert.Equal("result value", openResult);

                SessionHandler.Verify();
            }

            [InlineData(true)]
            [InlineData(false)]
            [Theory]
            public async Task Opening_Throws_Ensure_ProperState(bool canRecover)
            {
                string sessionid = "temp session";
                SessionErrorHandling.Setup(e => e.Handle(It.IsAny<ClientActionContext>(), It.IsAny<InvalidOperationException>()))
                    .Returns(canRecover ? ErrorHandlingResult.Recover : ErrorHandlingResult.Close);

                var pipeline = CreatePipeline(
                    (next, ctxt) =>
                        {
                            throw new InvalidOperationException();
                        });

                SetupSessionHandler(sessionid);

                var proxy = CreateProxy(pipeline);
                await Assert.ThrowsAsync<InvalidOperationException>(() => proxy.OpenSession("test"));

                if (canRecover)
                {
                    Assert.Equal(ProxyState.Ready, proxy.State);
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
                var pipeline = CreatePipeline(
                    (next, ctxt) =>
                        {
                            ctxt.ServerConnection = ConnectionDescriptor;
                            return next(ctxt);
                        });

                SetupSessionHandler(sessionid);
                SessionHandler.Setup(s => s.EnsureSession(It.IsAny<HttpRequestMessage>(), sessionid)).Verifiable();

                var proxy = CreateProxy(pipeline);
                await proxy.OpenSession("temp");
                await proxy.ExecuteAsync();

                SessionHandler.Verify();
            }

            [Fact]
            public async Task Close_Uninitialized_Ok()
            {
                var pipeline = CreatePipeline();
                var proxy = CreateProxy(pipeline);
                await proxy.CloseAsync();

                Assert.Equal(ProxyState.Closed, proxy.State);
            }

            [InlineData(true, ErrorHandlingResult.Close)]
            [InlineData(true, ErrorHandlingResult.Rethrow)]
            [InlineData(true, ErrorHandlingResult.Recover)]
            [InlineData(false, ErrorHandlingResult.Close)]
            [InlineData(false, ErrorHandlingResult.Rethrow)]
            [InlineData(false, ErrorHandlingResult.Recover)]
            [Theory]
            public async Task Execute_ThrowsError_Handle(bool recoverableProxy, ErrorHandlingResult  handlingResult)
            {
                var pipeline = CreatePipeline(
                      (next, ctxt) =>
                      {
                          ctxt.ServerConnection = ConnectionDescriptor;

                          if (ctxt.Action != ContractDescriptor.InitSession)
                          {
                              throw new InvalidOperationException();
                          }

                          return next(ctxt);
                      });
                var session = pipeline.Find<SessionMiddleware>();
                session.Recoverable = recoverableProxy;

                SetupSessionHandler("test session", true);
                SessionErrorHandling.Setup(r => r.Handle(It.IsAny<ClientActionContext>(), It.IsAny<Exception>())).Returns(
                    () =>
                        {
                            return handlingResult;
                        });

                var proxy = CreateProxy(pipeline);
                await proxy.OpenSession("param");
                await Assert.ThrowsAsync<InvalidOperationException>(proxy.ExecuteAsync);

                if (recoverableProxy)
                {
                    switch (handlingResult)
                    {
                        case ErrorHandlingResult.Close:
                            Assert.Equal(ProxyState.Closed, proxy.State);
                            break;
                        case ErrorHandlingResult.Recover:
                            Assert.Equal(ProxyState.Ready, proxy.State);
                            break;
                        case ErrorHandlingResult.Rethrow:
                            Assert.Equal(ProxyState.Open, proxy.State);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(handlingResult), handlingResult, null);
                    }
                }
                else
                {
                    switch (handlingResult)
                    {
                        case ErrorHandlingResult.Close:
                            Assert.Equal(ProxyState.Closed, proxy.State);
                            break;
                        case ErrorHandlingResult.Recover:
                            Assert.Equal(ProxyState.Closed, proxy.State);
                            break;
                        case ErrorHandlingResult.Rethrow:
                            Assert.Equal(ProxyState.Open, proxy.State);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(handlingResult), handlingResult, null);
                    }
                }
            }
        }
    }
}
