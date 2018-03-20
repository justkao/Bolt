using System;
using System.Net.Http;
using System.Threading.Tasks;
using Bolt.Client.Pipeline;
using Bolt.Metadata;
using Bolt.Pipeline;
using Bolt.Serialization;
using Moq;

namespace Bolt.Client.Test
{
    public abstract partial class SessionMiddlewareTest
    {
        public BoltOptions Options { get; } = new BoltOptions();

        public Mock<IClientSessionHandler> SessionHandler { get; } = new Mock<IClientSessionHandler>(MockBehavior.Strict);

        public Mock<IErrorHandling> SessionErrorHandling { get; } = new Mock<IErrorHandling>(MockBehavior.Strict);

        public Mock<ISerializer> Serializer { get; } = new Mock<ISerializer>(MockBehavior.Strict);

        public ConnectionDescriptor ConnectionDescriptor { get; } = new ConnectionDescriptor(new Uri("http://localhost"));

        public SessionContractMetadata SessionContract => BoltFramework.SessionMetadata.Resolve(typeof(ITestContract));

        public TestContractProxy CreateProxy(IClientPipeline pipeline)
        {
            return new TestContractProxy(pipeline);
        }

        public SessionContractMetadata ContractDescriptor => BoltFramework.SessionMetadata.Resolve(typeof(ITestContract));

        public IClientPipeline CreatePipeline(Func<ActionDelegate<ClientActionContext>, ClientActionContext, Task> next = null)
        {
            ClientPipelineBuilder builder = new ClientPipelineBuilder();
            builder.Use(new SessionMiddleware(SessionHandler.Object, SessionErrorHandling.Object));
            if (next != null)
            {
                builder.Use(next);
            }

            return builder.BuildClient();
        }

        protected void SetupSessionHandler(string sessionid, bool ensuresSession = false)
        {
            SessionHandler.Setup(s => s.GetSessionIdentifier(It.IsAny<HttpResponseMessage>())).Returns(sessionid);
            if (ensuresSession)
            {
                SessionHandler.Setup(s => s.EnsureSession(It.IsAny<HttpRequestMessage>(), sessionid)).Verifiable();
            }
        }

        public interface IInvokeCallback
        {
            void Handle(ClientActionContext context);
        }
    }
}
