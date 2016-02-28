using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bolt.Client;
using Bolt.Client.Proxy;
using Bolt.Server.IntegrationTest.Core;
using Moq;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class ErrorHandlingTest
    {
        public ErrorHandlingTest()
        {
            ErrorHandling = new Mock<IErrorHandling>();
            MessageHandler = new DummyMessageHandler();
            ClientConfiguration configuration = new ClientConfiguration().UseDynamicProxy();
            configuration.HttpMessageHandler = MessageHandler;

            Proxy = configuration.ProxyBuilder()
                .Recoverable(3, TimeSpan.FromMilliseconds(1), ErrorHandling.Object)
                .Url("http://localhost/Dummy")
                .Build<ITestContract>();
        }

        private DummyMessageHandler MessageHandler { get; set; }

        public ITestContract Proxy { get; set; }

        public Mock<IErrorHandling> ErrorHandling { get; set; }

        [Fact]
        public void Handle_EnsureCalled()
        {
            ErrorHandling.Setup(h => h.Handle(It.IsAny<ClientActionContext>(), It.IsAny<HttpRequestException>()))
                .Returns(ErrorHandlingResult.Rethrow)
                .Verifiable();

            Assert.Throws<HttpRequestException>(() => Proxy.SimpleMethod());
            ErrorHandling.Verify();
            ErrorHandling.Verify(h => h.Handle(It.IsAny<ClientActionContext>(), It.IsAny<HttpRequestException>()),
                Times.Exactly(1));

            Assert.Equal(1, MessageHandler.Called);
        }

        [Fact]
        public void Handle_Recover_ShouldThrowOnFinalTry()
        {
            ErrorHandling.Setup(h => h.Handle(It.IsAny<ClientActionContext>(), It.IsAny<HttpRequestException>()))
                .Returns(ErrorHandlingResult.Recover)
                .Verifiable();

            Assert.Throws<HttpRequestException>(() => Proxy.SimpleMethod());
            ErrorHandling.Verify(h => h.Handle(It.IsAny<ClientActionContext>(), It.IsAny<HttpRequestException>()),
                Times.Exactly(3 + 1));

            // 3 recoveries + final try
            Assert.Equal(3 + 1, MessageHandler.Called);
        }

        [Fact]
        public void Handle_Recover_ShouldBeClosedAfterFinalTry()
        {
            ErrorHandling.Setup(h => h.Handle(It.IsAny<ClientActionContext>(), It.IsAny<HttpRequestException>()))
                .Returns(ErrorHandlingResult.Recover)
                .Verifiable();

            Assert.Throws<HttpRequestException>(() => Proxy.SimpleMethod());
            Assert.Equal(ProxyState.Closed, ((IProxy)Proxy).State);
        }

        [Fact]
        public void Handle_Close_EnsureProxyClosed()
        {
            ErrorHandling.Setup(h => h.Handle(It.IsAny<ClientActionContext>(), It.IsAny<HttpRequestException>()))
                .Returns(ErrorHandlingResult.Close)
                .Verifiable();

            Assert.Throws<HttpRequestException>(() => Proxy.SimpleMethod());
            Assert.Equal(ProxyState.Closed, ((IProxy) Proxy).State);
            Assert.Throws<ProxyClosedException>(() => Proxy.SimpleMethod());

            ErrorHandling.Verify(h => h.Handle(It.IsAny<ClientActionContext>(), It.IsAny<HttpRequestException>()),
                Times.Exactly(1));

            Assert.Equal(1, MessageHandler.Called);
        }

        private class DummyMessageHandler : HttpMessageHandler
        {
            public int Called { get; set; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Called++;

                throw new HttpRequestException();
            }
        }
    }
}
