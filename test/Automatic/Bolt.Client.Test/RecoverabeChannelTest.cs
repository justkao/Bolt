using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bolt.Client.Channels;
using Moq;
using Xunit;

using System.Net.Http;
using System.Reflection;
using Bolt.Client.Filters;

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

        [Fact]
        public void ExplicitOpen_EnsureOpened()
        {
            var channel = Create();
            channel.Open();

            Assert.True(channel.IsOpened);
        }

        [Fact]
        public void CloseOpened_EnsureClosed()
        {
            var channel = Create();
            channel.Open();

            channel.Close();
            Assert.True(channel.IsClosed);
        }


        [Fact]
        public void Execute_EnsureOpened()
        {
            Mock<IRequestHandler> mocked = new Mock<IRequestHandler>();
            mocked.Setup(v => v.GetResponseAsync(It.IsAny<ClientActionContext>(), "test")).Returns(Task.FromResult(new ResponseDescriptor(new HttpResponseMessage(), new ClientActionContext(), "test")));

            var channel = Create(requestHandler: mocked);
        }

        protected virtual TestRecoverableChannel Create(Uri url = null, Mock<IRequestHandler> requestHandler = null)
        {
            url = url ?? new Uri("http://localhost");

            var endpointProvider = new Mock<IEndpointProvider>();
            endpointProvider.Setup(e => e.GetEndpoint(It.IsAny<Uri>(), It.IsAny<Type>(), It.IsAny<MethodInfo>())).Returns(url);

            return new TestRecoverableChannel(
                new JsonSerializer(), 
                new SingleServerProvider(url),
                requestHandler != null ? requestHandler.Object : Mock.Of<IRequestHandler>(),
                endpointProvider.Object);
        }

        protected class TestRecoverableChannel : RecoverableChannel
        {
            public bool BeforeSendingCalled { get; set; }

            public bool AfterReceivedCalled { get; set; }

            public bool DoHandleError { get; set; }

            public Exception Error { get; set; }

            public TestRecoverableChannel(ISerializer serializer, IServerProvider serverProvider, IRequestHandler requestHandler, IEndpointProvider endpointProvider)
                : base(serializer, serverProvider, requestHandler, endpointProvider, new List<IClientExecutionFilter>())
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
