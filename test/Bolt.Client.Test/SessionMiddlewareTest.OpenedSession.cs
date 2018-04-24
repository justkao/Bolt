using System;
using System.Threading.Tasks;
using Bolt.Client.Pipeline;
using Moq;
using Xunit;

namespace Bolt.Client.Test
{
    public abstract partial class SessionMiddlewareTest
    {
        public class OpenedSession : SessionMiddlewareTest
        {
            private const string SessionId = "test session";

            private Mock<IInvokeCallback> _callback = new Mock<IInvokeCallback>();

            private Mock<IProxyEvents> _events = new Mock<IProxyEvents>(MockBehavior.Loose);

            public OpenedSession()
            {
                _events.Setup(p => p.OnProxyOpenedAsync(It.IsAny<IProxy>())).Returns(Task.CompletedTask);

                Pipeline = CreatePipeline(
                    (next, ctxt) =>
                    {
                        _callback.Object.Handle(ctxt);
                        ctxt.ServerConnection = ConnectionDescriptor;
                        return next(ctxt);
                    });

                SetupSessionHandler(SessionId, true);
                Proxy = CreateProxy(Pipeline);
                Proxy.Events = _events.Object;
                Proxy.OpenSession("test").GetAwaiter().GetResult();
            }

            public IClientPipeline Pipeline { get; set; }

            public TestContractProxy Proxy { get; set; }

            [Fact]
            public void Ensure_Open()
            {
                Assert.Equal(ProxyState.Open, Proxy.State);
            }

            [Fact]
            public async Task Close_Ok()
            {
                await Proxy.CloseSession("Test");

                Assert.Equal(ProxyState.Closed, Proxy.State);
            }

            [Fact]
            public async Task Close_EnsureEventCalled()
            {
                _events.Setup(p => p.OnProxyClosedAsync(It.IsAny<IProxy>())).Returns(Task.CompletedTask).Verifiable();
                await Proxy.CloseSession("Test");

                Assert.Equal(ProxyState.Closed, Proxy.State);
                _events.Verify();
            }

            [Fact]
            public async Task Close_CloseEventThrows_EnsureClosed()
            {
                _events.Setup(p => p.OnProxyClosedAsync(It.IsAny<IProxy>())).Callback<IProxy>(p => throw new InvalidOperationException("Forced error")).Returns(Task.CompletedTask);
                await Assert.ThrowsAsync<InvalidOperationException>(() => Proxy.CloseSession("Test"));

                Assert.Equal(ProxyState.Closed, Proxy.State);
            }

            [Fact]
            public async Task Close_EnsureCalled()
            {
                _callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>())).Callback<ClientActionContext>(
                    c =>
                        {
                            Assert.Equal(ContractDescriptor.DestroySession.Action, c.Action.Action);
                        }).Verifiable();

                await Proxy.CloseSession("Test");

                _callback.Verify();
            }

            [Fact]
            public async Task Close_EnsureProperParameters()
            {
                _callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>())).Callback<ClientActionContext>(
                    c =>
                    {
                        Assert.Equal("temp", c.Parameters[0]);
                    }).Verifiable();

                await Proxy.CloseSession("temp");

                _callback.Verify();
            }

            [Fact]
            public async Task Close_EnsureProperResult()
            {
                _callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>())).Callback<ClientActionContext>(
                    c =>
                        {
                            c.ActionResult = "test";
                        }).Verifiable();

                var result = await Proxy.CloseSession("temp");

                Assert.Equal("test", result);
                _callback.Verify();
            }

            [Fact]
            public async Task Close_EnsureProperConnection()
            {
                var connection = Pipeline.Find<SessionMiddleware>().GetSession(Proxy);

                Assert.NotNull(connection);

                _callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>())).Callback<ClientActionContext>(
                    c =>
                    {
                        Assert.Equal(connection.ServerConnection, c.ServerConnection);
                    }).Verifiable();

                await Proxy.CloseSession("Test");

                _callback.Verify();
            }

            [Fact]
            public async Task Close_Twice_Ok()
            {
                await Proxy.CloseSession("Test");

                Assert.Equal(ProxyState.Closed, Proxy.State);
            }

            [Fact]
            public Task CloseOnIProxy_FailForRequiredParameters_Ok()
            {
               return Assert.ThrowsAsync<BoltClientException>(() => Proxy.CloseAsync());
            }

            [Fact]
            public void Dispose_FailForRequiredParameters_Ok()
            {
                Assert.Throws<BoltClientException>(() => Proxy.Dispose());
            }
        }
    }
}
