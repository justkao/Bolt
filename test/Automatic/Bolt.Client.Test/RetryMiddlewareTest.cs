using System;
using System.Threading.Tasks;
using Bolt.Client.Pipeline;
using Moq;
using Xunit;

namespace Bolt.Client.Test
{
    public class RetryMiddlewareTest
    {
        public Mock<IErrorHandling> ErrorHandling { get; } = new Mock<IErrorHandling>(MockBehavior.Strict);

        public Mock<IInvokeCallback> Callback { get; } = new Mock<IInvokeCallback>(MockBehavior.Strict);

        [Fact]
        public void Ok_NoRetries()
        {
            var pipeline = CreatePipeline(10);
            var proxy = CreateProxy(pipeline);
            Callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>()));
            proxy.Execute("");
        }

        [InlineData(true, ErrorHandlingResult.Close)]
        [InlineData(true, ErrorHandlingResult.Rethrow)]
        [InlineData(true, ErrorHandlingResult.Recover)]
        [InlineData(false, ErrorHandlingResult.Close)]
        [InlineData(false, ErrorHandlingResult.Rethrow)]
        [InlineData(false, ErrorHandlingResult.Recover)]
        [Theory]
        public async Task Error_EnsureProperProxyState(bool retry, ErrorHandlingResult result)
        {
            var pipeline = CreatePipeline(retry ? 1 : 0);
            var proxy = CreateProxy(pipeline);
            bool called = false;

            Callback.Setup(c => c.Handle(It.IsAny<ClientActionContext>())).Callback<ClientActionContext>(
                c =>
                    {
                        if (called)
                        {
                            if (result == ErrorHandlingResult.Rethrow)
                            {
                                throw new InvalidOperationException("This method sould not be called again.");
                            }

                            return;
                        }

                        called = true;
                        throw new InvalidOperationException();
                    }).Verifiable();

            ErrorHandling.Setup(e => e.Handle(It.IsAny<ClientActionContext>(), It.IsAny<Exception>())).Returns(result);

            if (result != ErrorHandlingResult.Recover)
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() => proxy.ExecuteAsync());
            }
            else
            {
                if (retry)
                {
                    await proxy.ExecuteAsync();
                }
                else
                {
                    await Assert.ThrowsAsync<InvalidOperationException>(() => proxy.ExecuteAsync());
                }
            }

            switch (result)
            {
                case ErrorHandlingResult.Close:
                    Assert.Equal(ProxyState.Closed, proxy.State);
                    break;
                case ErrorHandlingResult.Recover:
                    if (retry)
                    {
                        Assert.Equal(ProxyState.Open, proxy.State);
                    }
                    else
                    {
                        Assert.Equal(ProxyState.Closed, proxy.State);
                    }
                    break;
                case ErrorHandlingResult.Rethrow:
                    Assert.Equal(ProxyState.Open, proxy.State);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }

        public TestContractProxy CreateProxy(IClientPipeline pipeline)
        {
            return new TestContractProxy(pipeline);
        }

        public IClientPipeline CreatePipeline(int retries)
        {
            ClientPipelineBuilder builder = new ClientPipelineBuilder();
            builder.Use(new RetryRequestMiddleware(ErrorHandling.Object) {Retries = retries});
            builder.Use(
                (next, ctxt) =>
                    {
                        Callback.Object.Handle(ctxt);
                        return next(ctxt);
                    });

            return builder.BuildClient();
        }

        public interface IInvokeCallback
        {
            void Handle(ClientActionContext context);
        }
    }
}
