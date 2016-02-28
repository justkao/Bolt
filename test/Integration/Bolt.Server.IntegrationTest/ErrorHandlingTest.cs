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
            ClientConfiguration configuration = new ClientConfiguration().UseDynamicProxy();
            configuration.HttpMessageHandler = new DummyMessageHandler();

            Proxy = configuration.ProxyBuilder()
                .Recoverable(3, TimeSpan.FromMilliseconds(1), ErrorHandling.Object)
                .Url("http://localhost/Dummy")
                .Build<ITestContract>();
        }

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
        }

        [Fact]
        public void Handle_Recover_ShouldThrowOnFinalTry()
        {
            ErrorHandling.Setup(h => h.Handle(It.IsAny<ClientActionContext>(), It.IsAny<HttpRequestException>()))
                .Returns(ErrorHandlingResult.Recover)
                .Verifiable();

            Assert.Throws<HttpRequestException>(() => Proxy.SimpleMethod());

            ErrorHandling.Verify(h => h.Handle(It.IsAny<ClientActionContext>(), It.IsAny<HttpRequestException>()),
                Times.Exactly(3));
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
        }

        private class DummyMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new HttpRequestException();
            }
        }
    }
}
