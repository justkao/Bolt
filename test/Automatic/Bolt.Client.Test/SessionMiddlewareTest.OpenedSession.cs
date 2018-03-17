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

            private Mock<IInvokeCallback> Callback = new Mock<IInvokeCallback>();

            public OpenedSession()
            {
                Pipeline = CreatePipeline(
                    (next, ctxt) =>
                    {
                        Callback.Object.Handle(ctxt);
                        ctxt.ServerConnection = ConnectionDescriptor;
                        return next(ctxt);
                    });

                SetupSessionHandler(SessionId, true);
                Proxy = CreateProxy(Pipeline);
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
            public async Task Close_EnsureCalled()
            {
                Callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>())).Callback<ClientActionContext>(
                    c =>
                        {
                            Assert.Equal(ContractDescriptor.DestroySession.Action, c.Action);
                        }).Verifiable();

                await Proxy.CloseSession("Test");

                Callback.Verify();
            }

            [Fact]
            public async Task Close_EnsureProperParameters()
            {
                Callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>())).Callback<ClientActionContext>(
                    c =>
                    {
                        Assert.Equal("temp", c.Parameters[0]);
                    }).Verifiable();

                await Proxy.CloseSession("temp");

                Callback.Verify();
            }

            [Fact]
            public async Task Close_EnsureProperResult()
            {

                Callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>())).Callback<ClientActionContext>(
                    c =>
                        {
                            c.ActionResult = "test";
                        }).Verifiable();

                var result = await Proxy.CloseSession("temp");

                Assert.Equal("test", result);
                Callback.Verify();
            }

            [Fact]
            public async Task Close_EnsureProperConnection()
            {
                var connection = Pipeline.Find<SessionMiddleware>().GetSession(Proxy);

                Assert.NotNull(connection);

                Callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>())).Callback<ClientActionContext>(
                    c =>
                    {
                        Assert.Equal(connection.ServerConnection, c.ServerConnection);
                    }).Verifiable();

                await Proxy.CloseSession("Test");

                Callback.Verify();
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
