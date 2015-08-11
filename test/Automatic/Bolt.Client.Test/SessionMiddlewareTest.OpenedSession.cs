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
        public class OpenedSession : SessionMiddlewareTest
        {
            private const string SessionId = "test session";

            private Mock<IInvokeCallback> Callback = new Mock<IInvokeCallback>();

            public OpenedSession()
            {
                Pipeline = CreatePipeline(
                    (next, ctxt) =>
                    {
                        Callback.Object.Handle(ctxt);
                        ctxt.ServerConnection = DefaultDescriptor;
                        return next(ctxt);
                    });

                SetupSessionHandler(SessionId);
                Proxy = CreateProxy(Pipeline);
                Proxy.Open();
            }

            public IPipeline<ClientActionContext> Pipeline { get; set; }

            public TestContractProxy Proxy { get; set; }

            [Fact]
            public void Ensure_Open()
            {
                Assert.Equal(ProxyState.Open, Proxy.State);
            }

            [Fact]
            public async Task Close_Ok()
            {
                await Proxy.CloseAsync();

                Assert.Equal(ProxyState.Closed, Proxy.State);
            }

            [Fact]
            public async Task Close_EnsureCalled()
            {
                Callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>())).Callback<ClientActionContext>(
                    c =>
                        {
                            Assert.Equal(BoltFramework.DestroySessionAction, c.Action);
                        }).Verifiable();

                await Proxy.CloseAsync();

                Callback.Verify();
            }

            [Fact]
            public async Task Close_EnsureProperParameters()
            {
                Pipeline.Find<SessionMiddleware>().DestroySessionParameters = new DestroySessionParameters();

                Callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>())).Callback<ClientActionContext>(
                    c =>
                    {
                        Assert.Equal(Pipeline.Find<SessionMiddleware>().DestroySessionParameters, c.Parameters[0]);
                    }).Verifiable();

                await Proxy.CloseAsync();

                Callback.Verify();
            }

            [Fact]
            public async Task Close_EnsureProperResult()
            {
                DestroySessionResult result = new DestroySessionResult();

                Callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>())).Callback<ClientActionContext>(
                    c =>
                        {
                            c.ActionResult = result;
                        }).Verifiable();

                await Proxy.CloseAsync();

                Assert.Equal(result, Pipeline.Find<SessionMiddleware>().DestroySessionResult);
                Callback.Verify();
            }

            [Fact]
            public async Task Close_EnsureProperConnection()
            {
                var connection = Pipeline.Find<SessionMiddleware>().ServerConnection;
                Assert.NotNull(connection);

                Callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>())).Callback<ClientActionContext>(
                    c =>
                    {
                        Assert.Equal(connection, c.ServerConnection);
                    }).Verifiable();

                await Proxy.CloseAsync();

                Callback.Verify();
            }

            [Fact]
            public async Task Close_Twice_Ok()
            {
                await Proxy.CloseAsync();

                Assert.Equal(ProxyState.Closed, Proxy.State);
            }

            [Fact]
            public void Dispose_Ok()
            {
                Proxy.Dispose();

                Assert.Equal(ProxyState.Closed, Proxy.State);
            }

            [Fact]
            public void Dispose_Twice_Ok()
            {
                Proxy.Dispose();

                Assert.Equal(ProxyState.Closed, Proxy.State);
            }
        }
    }
}
