using System;
using System.Net.Http;
using System.Threading.Tasks;
using Bolt.Client.Pipeline;
using Bolt.Pipeline;
using Bolt.Session;
using Moq;

namespace Bolt.Client.Test
{
    public abstract partial class SessionMiddlewareTest
    {
        public BoltOptions Options { get; } = new BoltOptions();

        public Mock<IClientSessionHandler> SessionHandler = new Mock<IClientSessionHandler>(MockBehavior.Strict);

        public Mock<IErrorHandling> SessionErrorHandling = new Mock<IErrorHandling>(MockBehavior.Strict);

        public Mock<ISerializer> Serializer = new Mock<ISerializer>(MockBehavior.Strict);

        public ConnectionDescriptor ConnectionDescriptor = new ConnectionDescriptor(new Uri("http://localhost"));

        public SessionContractDescriptor SessionContract => BoltFramework.GetSessionDescriptor(typeof(ITestContract));

        protected void SetupSessionHandler(string sessionid, bool ensuresSession = false)
        {
            SessionHandler.Setup(s => s.GetSessionIdentifier(It.IsAny<HttpResponseMessage>())).Returns(sessionid);
            if (ensuresSession)
            {
                SessionHandler.Setup(s => s.EnsureSession(It.IsAny<HttpRequestMessage>(), sessionid)).Verifiable();
            }
        }

        public TestContractProxy CreateProxy(IClientPipeline pipeline)
        {
            return new TestContractProxy(pipeline);
        }

        public SessionContractDescriptor ContractDescriptor => BoltFramework.GetSessionDescriptor(typeof(ITestContract));

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

        public interface IInvokeCallback
        {
            void Handle(ClientActionContext context);
        }
    }
}
