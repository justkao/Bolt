using System;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Common;
using Bolt.Server.InstanceProviders;
using Microsoft.AspNet.Http.Internal;
using Moq;

namespace Bolt.Server.Test
{
    public abstract class StatefullinstanceProviderTestBase
    {
        protected const string SessionHeader = "test-session-id";

        protected StatefullinstanceProviderTestBase()
        {
            Contract = new MockContractDescriptor();
            Mock = new Mock<IInstanceProviderActions>(MockBehavior.Loose);
            Subject = CreateSubject();
        }

        protected MockStateFullInstanceProvider Subject { get; set; }

        protected Mock<IInstanceProviderActions> Mock { get; set; }

        public interface IInstanceProviderActions
        {
            object CreateInstance(ServerActionContext context, Type type);

            void OnInstanceCreated(ServerActionContext context, string sessionId);

            void OnInstanceReleased(ServerActionContext context, string sessionId);

            string GenerateSessionid();
        }

        protected class MockStateFullInstanceProvider : StateFullInstanceProvider
        {
            private readonly Mock<IInstanceProviderActions> _actions;
            private readonly MemorySessionFactory _factory;

            public MockStateFullInstanceProvider(MockContractDescriptor contract,
                Mock<IInstanceProviderActions> actions)
                : this(
                    contract, actions,
                    new MemorySessionFactory(new BoltServerOptions {SessionHeader = SessionHeader},
                        new ServerSessionHandlerInternal(actions,
                            new BoltServerOptions {SessionHeader = SessionHeader})))
            {
            }

            private MockStateFullInstanceProvider(MockContractDescriptor contract, Mock<IInstanceProviderActions> actions, MemorySessionFactory factory) : base(contract.Init, contract.Destroy, factory)
            {
                _factory = factory;
                _actions = actions;
            }

            public int LocalCount => _factory.Count;

            protected override object CreateInstance(ServerActionContext context, Type type)
            {
                return _actions.Object.CreateInstance(context, type);
            }

            protected override Task OnInstanceCreatedAsync(ServerActionContext context, string sessionId)
            {
                _actions.Object.OnInstanceCreated(context, sessionId);
                return CompletedTask.Done;
            }

            protected override Task OnInstanceReleasedAsync(ServerActionContext context, string sessionId)
            {
                _actions.Object.OnInstanceReleased(context, sessionId);
                return CompletedTask.Done;
            }

            private class ServerSessionHandlerInternal : ServerSessionHandler
            {
                private readonly Mock<IInstanceProviderActions> _actions;

                public ServerSessionHandlerInternal(Mock<IInstanceProviderActions> actions, BoltServerOptions options) : base(options)
                {
                    _actions = actions;
                }

                protected override string GenerateIdentifier()
                {
                    return _actions.Object.GenerateSessionid() ?? Guid.NewGuid().ToString();
                }
            }
        }

        protected virtual ServerActionContext CreateContext(MethodInfo action)
        {
            return new ServerActionContext()
            {
                HttpContext = new DefaultHttpContext(),
                Action = action
            };
        }

        protected virtual MockStateFullInstanceProvider CreateSubject()
        {
            return new MockStateFullInstanceProvider(Contract, Mock);
        }

        protected MockContractDescriptor Contract { get; }
    }
}