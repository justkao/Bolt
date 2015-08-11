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
                await Proxy.ExecuteAsync();
                Assert.Equal(ProxyState.Open, Proxy.State);
            }

            [Fact]
            public async Task Execute_EnsureOpenedAndExecuted()
            {
                await Proxy.ExecuteAsync();

                Callback.Verify(s => s.Handle(It.IsAny<ClientActionContext>()), Times.Exactly(2));
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

                await Assert.ThrowsAsync<InvalidOperationException>(Proxy.ExecuteAsync);
                if (recoverableProxy)
                {
                    switch (handlingResult)
                    {
                        case ErrorHandlingResult.Close:
                            Assert.Equal(ProxyState.Closed, Proxy.State);
                            break;
                        case ErrorHandlingResult.Recover:
                            Assert.Equal(ProxyState.Uninitialized, Proxy.State);
                            break;
                        case ErrorHandlingResult.Rethrow:
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
                        case ErrorHandlingResult.Close:
                            Assert.Equal(ProxyState.Closed, Proxy.State);
                            break;
                        case ErrorHandlingResult.Recover:
                            Assert.Equal(ProxyState.Closed, Proxy.State);
                            break;
                        case ErrorHandlingResult.Rethrow:
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
