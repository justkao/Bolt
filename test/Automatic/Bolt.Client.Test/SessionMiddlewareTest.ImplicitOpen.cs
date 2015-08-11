using System;
using System.Threading.Tasks;

using Bolt.Client.Pipeline;
using Bolt.Pipeline;

using Moq;

using Xunit;

namespace Bolt.Client.Test
{
    public abstract partial class SessionMiddlewareTest
    {
        public class ImlicitOpen : SessionMiddlewareTest
        {
            private const string SessionId = "test session";

            private Mock<IInvokeCallback> Callback = new Mock<IInvokeCallback>();

            public ImlicitOpen()
            {
                Pipeline = CreatePipeline(
                    (next, ctxt) =>
                        {
                            Callback.Object.Handle(ctxt);
                            ctxt.Connection = DefaultDescriptor;
                            return next(ctxt);
                        });

                SetupSessionHandler(SessionId);
                Proxy = CreateProxy(Pipeline);
            }

            public IPipeline<ClientActionContext> Pipeline { get; set; }

            public TestContractProxy Proxy { get; set; }

            [Fact]
            public async Task Execute_EnsureSessionOpened()
            {
                await Proxy.Execute();
                Assert.Equal(ProxyState.Open, Proxy.State);
            }

            [Fact]
            public async Task Execute_EnsureOpenedAndExecuted()
            {
                await Proxy.Execute();

                Callback.Verify(s => s.Handle(It.IsAny<ClientActionContext>()), Times.Exactly(2));
            }

            [InlineData(true, SessionHandlingResult.Close)]
            [InlineData(true, SessionHandlingResult.Rethrow)]
            [InlineData(true, SessionHandlingResult.Recover)]
            [InlineData(false, SessionHandlingResult.Close)]
            [InlineData(false, SessionHandlingResult.Rethrow)]
            [InlineData(false, SessionHandlingResult.Recover)]
            [Theory]
            public async Task Execute_ThrowsError_Handle(bool recoverableProxy, SessionHandlingResult  handlingResult)
            {
                Pipeline.Find<SessionMiddleware>().Recoverable = recoverableProxy;
                Callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>())).Callback<ClientActionContext>(
                    v =>
                        {
                            if (v.Action != BoltFramework.InitSessionAction)
                            {
                                throw new InvalidOperationException();
                            }
                        });
                SessionErrorHandling.Setup(r => r.Handle(It.IsAny<ClientActionContext>(), It.IsAny<Exception>())).Returns(
                    () =>
                        {
                            return handlingResult;
                        });

                await Assert.ThrowsAsync<InvalidOperationException>(Proxy.Execute);
                if (recoverableProxy)
                {
                    switch (handlingResult)
                    {
                        case SessionHandlingResult.Close:
                            Assert.Equal(ProxyState.Closed, Proxy.State);
                            break;
                        case SessionHandlingResult.Recover:
                            Assert.Equal(ProxyState.Uninitialized, Proxy.State);
                            break;
                        case SessionHandlingResult.Rethrow:
                            Assert.Equal(ProxyState.Open, Proxy.State);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(handlingResult), handlingResult, null);
                    }
                }
                else
                {
                    switch (handlingResult)
                    {
                        case SessionHandlingResult.Close:
                            Assert.Equal(ProxyState.Closed, Proxy.State);
                            break;
                        case SessionHandlingResult.Recover:
                            Assert.Equal(ProxyState.Closed, Proxy.State);
                            break;
                        case SessionHandlingResult.Rethrow:
                            Assert.Equal(ProxyState.Open, Proxy.State);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(handlingResult), handlingResult, null);
                    }
                }
            }
        }
    }
}
