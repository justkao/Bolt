using System;
using System.Net.Http;
using System.Threading.Tasks;

using Bolt.Client.Channels;
using Bolt.Client.Pipeline;
using Bolt.Pipeline;

using Moq;

namespace Bolt.Client.Test
{
    public abstract partial class SessionMiddlewareTest
    {
        public BoltOptions Options { get; } = new BoltOptions();

        public Mock<IClientSessionHandler> SessionHandler = new Mock<IClientSessionHandler>(MockBehavior.Strict);

        public Mock<IErrorHandling> SessionErrorHandling = new Mock<IErrorHandling>(MockBehavior.Strict);

        public Mock<ISerializer> Serializer = new Mock<ISerializer>(MockBehavior.Strict);

        public ConnectionDescriptor DefaultDescriptor = new ConnectionDescriptor(new Uri("http://localhost"));

        protected void SetupSessionHandler(string sessionid)
        {
            SessionHandler.Setup(s => s.GetSessionIdentifier(It.IsAny<HttpResponseMessage>())).Returns(sessionid);
            SessionHandler.Setup(s => s.EnsureSession(It.IsAny<HttpRequestMessage>(), sessionid)).Verifiable();
        }

        public TestContractProxy CreateProxy(IPipeline<ClientActionContext> pipeline)
        {
            return new TestContractProxy(pipeline);
        }

        public IPipeline<ClientActionContext> CreatePipeline(Func<ActionDelegate<ClientActionContext>, ClientActionContext, Task> next = null)
        {
            PipelineBuilder<ClientActionContext> builder = new PipelineBuilder<ClientActionContext>();
            builder.Use(new SessionMiddleware(SessionHandler.Object, SessionErrorHandling.Object));
            if (next != null)
            {
                builder.Use(next);
            }

            return builder.Build();
        } 

        public interface IInvokeCallback
        {
            void Handle(ClientActionContext context);
        }
    }
}
