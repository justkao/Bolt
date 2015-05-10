using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bolt.Client.Channels;
using Moq;
using Xunit;

namespace Bolt.Client.Test
{
    public class RecoverabeChannelTest
    {
        [Fact]
        public void New_IsNotOpened()
        {
            var channel = Create();

            Assert.False(channel.IsOpened);
        }

        [Fact]
        public void New_IsNotClosed()
        {
            var channel = Create();

            Assert.False(channel.IsClosed);
        }

        private TestRecoverableChannel Create(Uri url = null, Mock<IRequestHandler> requestHandler = null)
        {
            url = url ?? new Uri("http://localhost");

            var endpointProvider = new Mock<IEndpointProvider>();
            endpointProvider.Setup(e => e.GetEndpoint(It.IsAny<Uri>(), It.IsAny<ActionDescriptor>())).Returns(url);

            return new TestRecoverableChannel(
                new SingleServerProvider(url),
                requestHandler != null ? requestHandler.Object : Mock.Of<IRequestHandler>(),
                endpointProvider.Object);
        }

        private class TestRecoverableChannel : RecoverableChannel
        {
            public bool BeforeSendingCalled { get; set; }

            public bool AfterReceivedCalled { get; set; }

            public bool DoHandleError { get; set; }

            public Exception Error { get; set; }

            public TestRecoverableChannel(IServerProvider serverProvider, IRequestHandler requestHandler, IEndpointProvider endpointProvider)
                : base(serverProvider, requestHandler, endpointProvider)
            {
            }

            protected override void BeforeSending(ClientActionContext context)
            {
                BeforeSendingCalled = true;
                base.BeforeSending(context);
            }

            protected override void AfterReceived(ClientActionContext context)
            {
                AfterReceivedCalled = true;
                base.AfterReceived(context);
            }

            protected override bool HandleError(ClientActionContext context, Exception error)
            {
                Error = error;
                return DoHandleError;
            }
        }
    }
}
