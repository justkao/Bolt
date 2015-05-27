using Bolt.Server.InstanceProviders;
using System;
using System.Reflection;
using Microsoft.AspNet.Http;
using Xunit;
using Moq;
using System.Collections.Generic;
using Microsoft.AspNet.Http.Internal;
using System.Threading.Tasks;

namespace Bolt.Server.Test
{
    public interface IMockContract
    {
        void Init();

        void Action();

        void Destroy();
    }

    public class MockContractDescriptor : ContractDescriptor
    {
        public MockContractDescriptor() : base(typeof(IMockContract))
        {
            Init = Add("Init", typeof(Empty), typeof(IMockContract).GetTypeInfo().GetDeclaredMethod("Init"));
            Action = Add("Action", typeof(Empty), typeof(IMockContract).GetTypeInfo().GetDeclaredMethod("Action"));
            Destroy = Add("Destroy", typeof(Empty), typeof(IMockContract).GetTypeInfo().GetDeclaredMethod("Destroy"));
        }

        public ActionDescriptor Init { get; private set; }

        public ActionDescriptor Action { get; private set; }

        public ActionDescriptor Destroy { get; private set; }
    }

    public class StatefullinstanceProviderTests
    {
        public class ReleaseInstanceWithoutSession : StatefullinstanceProviderTestBase
        {
            [Fact]
            public async Task Destroy_ShouldNotThrow()
            {
                var ctxt = CreateContext(Contract.Destroy);

                await Subject.ReleaseInstanceAsync(ctxt, It.IsAny<object>(), null);
            }
        }

        public class GetInstanceWithoutSession : StatefullinstanceProviderTestBase
        {
            [Fact]
            public void Action_ShouldThrow()
            {
                var ctxt = CreateContext(Contract.Action);

                Assert.Throws<SessionHeaderNotFoundException>(() => Subject.GetInstanceAsync(ctxt, typeof(IMockContract)).GetAwaiter().GetResult());
            }

            [Fact]
            public async Task Init_Ok()
            {
                var ctxt = CreateContext(Contract.Init);
                SetupInit(ctxt);

                await Subject.GetInstanceAsync(ctxt, typeof(IMockContract));

                Mock.Verify();
            }

            [Fact]
            public async Task Init_EnsureInstanceCreatedAndCached()
            {
                var ctxt = CreateContext(Contract.Init);
                SetupInit(ctxt);

                await Subject.GetInstanceAsync(ctxt, typeof(IMockContract));

                Assert.Equal(1, Subject.LocalCount);
            }

            [Fact]
            public async Task Init_EnsureSessionHeaderCreated()
            {
                var ctxt = CreateContext(Contract.Init);
                SetupInit(ctxt);

                await Subject.GetInstanceAsync(ctxt, typeof(IMockContract));

                Assert.NotNull(ctxt.HttpContext.Response.Headers[SessionHeader]);
            }

            [Fact]
            public async Task Init_EnsureSessionHeaderValue()
            {
                var ctxt = CreateContext(Contract.Init);
                SetupInit(ctxt, "testsession");

                await Subject.GetInstanceAsync(ctxt, typeof(IMockContract));

                Assert.Equal("testsession", ctxt.HttpContext.Response.Headers[SessionHeader]);
            }

            [Fact]
            public async Task Init_EnsureInstance()
            {
                var ctxt = CreateContext(Contract.Init);
                var instance = new object();

                SetupInit(ctxt, "testsession", instance);

                var result = await Subject.GetInstanceAsync(ctxt, typeof(IMockContract));

                Assert.Equal(instance, result);
            }

            [Fact]
            public async Task Init_EnsureOnInstanceCreatedCalled()
            {
                var ctxt = CreateContext(Contract.Init);
                var instance = new object();

                SetupInit(ctxt, "testsession", instance);
                Mock.Setup(o => o.OnInstanceCreated(ctxt, "testsession")).Verifiable();

                await Subject.GetInstanceAsync(ctxt, typeof(IMockContract));

                Mock.Verify();
            }

            [Fact]
            public async Task Init_EnsureInstanceNotCreated()
            {
                var ctxt = CreateContext(Contract.Action);

                await Assert.ThrowsAsync<SessionHeaderNotFoundException>(() => Subject.GetInstanceAsync(ctxt, typeof(IMockContract)));

                Mock.Verify(v => v.CreateInstance(ctxt, typeof(IMockContract)), Times.Never);
            }

            [Fact]
            public async Task Destroy_Throws()
            {
                var ctxt = CreateContext(Contract.Destroy);
                await Assert.ThrowsAsync<SessionHeaderNotFoundException>(() => Subject.GetInstanceAsync(ctxt, typeof(IMockContract)));
            }

            private void SetupInit(ServerActionContext ctxt, string session = "testsession", object instance = null)
            {
                Mock.Setup(o => o.CreateInstance(ctxt, typeof(IMockContract))).Returns(instance ?? new object()).Verifiable();
                Mock.Setup(o => o.CreateNewSession()).Returns(session).Verifiable();
            }
        }

        public class GetInstanceWithExistingSession : StatefullinstanceProviderTestBase
        {
            private string SessionHeaderValue = "session value";

            private object _instance;

            public GetInstanceWithExistingSession()
            {
                var ctxt = CreateContext(Contract.Init);
                _instance = new object();

                Mock.Setup(o => o.CreateInstance(It.IsAny<ServerActionContext>(), typeof(IMockContract))).Returns(_instance).Verifiable();
                Mock.Setup(o => o.CreateNewSession()).Returns(SessionHeaderValue).Verifiable();

                Subject.GetInstanceAsync(ctxt, typeof(IMockContract)).GetAwaiter().GetResult();
            }

            [Fact]
            public async Task Init_ExistingSession_Ok()
            {
                var ctxt = CreateContext(Contract.Init);

                var result = await Subject.GetInstanceAsync(ctxt, typeof(IMockContract));
                Assert.Equal(_instance, result);
            }

            [Fact]
            public async Task Action_EnsureCorrectInstance()
            {
                var ctxt = CreateContext(Contract.Action);

                var result = await Subject.GetInstanceAsync(ctxt, typeof(IMockContract));

                Assert.Equal(_instance, result);
            }

            [Fact]
            public async Task Action_DifferentSession_EnsureThrows()
            {
                SessionHeaderValue = "another session";
                var ctxt = CreateContext(Contract.Action);

                await Assert.ThrowsAsync<SessionNotFoundException>(() => Subject.GetInstanceAsync(ctxt, typeof(IMockContract)));
            }

            [Fact]
            public async Task Destroy_DifferentSession_EnsureThrows()
            {
                SessionHeaderValue = "another session";
                var ctxt = CreateContext(Contract.Destroy);

                await Assert.ThrowsAsync<SessionNotFoundException>(() => Subject.GetInstanceAsync(ctxt, typeof(IMockContract)));
            }

            protected override ServerActionContext CreateContext(ActionDescriptor descriptor)
            {
                var ctxt = base.CreateContext(descriptor);
                ctxt.HttpContext.Request.Headers[SessionHeader] = SessionHeaderValue;
                return ctxt;
            }
        }

        public class ReleaseInstanceWithExistingSession : StatefullinstanceProviderTestBase
        {
            private string SessionHeaderValue = "session value";

            private object _instance;

            private ServerActionContext _context;

            public ReleaseInstanceWithExistingSession()
            {
                _context = CreateContext(Contract.Init);
                _instance = new InstanceObject();

                Mock.Setup(o => o.CreateInstance(It.IsAny<ServerActionContext>(), typeof(IMockContract))).Returns(_instance).Verifiable();
                Mock.Setup(o => o.CreateNewSession()).Returns(SessionHeaderValue).Verifiable();

                Subject.GetInstanceAsync(_context, typeof(IMockContract)).GetAwaiter().GetResult();
            }

            [Fact]
            public async Task Release_DestroyAction_Ok()
            {
                var ctxt = CreateContext(Contract.Destroy);

                SetupRelease(ctxt);

                await Subject.ReleaseInstanceAsync(ctxt, _instance, null);

                Mock.Verify();
            }

            [Fact]
            public async Task  Release_DestroyAction_InstanceDestroyed()
            {
                var ctxt = CreateContext(Contract.Destroy);

                SetupRelease();

                await Subject.ReleaseInstanceAsync(ctxt, _instance, null);

                Assert.Equal(0, Subject.LocalCount);
            }

            [Fact]
            public async Task Release_DestroyAction_OnInstanceReleasedCalled()
            {
                var ctxt = CreateContext(Contract.Destroy);

                SetupRelease(ctxt);

                await Subject.ReleaseInstanceAsync(ctxt, _instance, null);

                Mock.Verify();
            }

            [Fact]
            public async Task Release_Action_InstanceNotDestroyed()
            {
                var ctxt = CreateContext(Contract.Action);

                SetupRelease();

                await Subject.ReleaseInstanceAsync(ctxt, _instance, null);

                Assert.Equal(1, Subject.LocalCount);
            }

            [Fact]
            public async Task Release_DestroyAction_InstanceDisposed()
            {
                var ctxt = CreateContext(Contract.Destroy);
                SetupRelease();

                await Subject.ReleaseInstanceAsync(ctxt, _instance, null);

                Assert.True((_instance as InstanceObject).Disposed);
            }

            [Fact]
            public async Task Release_Action_InstanceNotDisposed()
            {
                var ctxt = CreateContext(Contract.Action);
                SetupRelease();

                await Subject.ReleaseInstanceAsync(ctxt, _instance, null);

                Assert.False((_instance as InstanceObject).Disposed);
            }

            [Fact]
            public async Task ReleaseWhenError_InitAction_InstanceDisposed()
            {
                var ctxt = CreateContext(Contract.Init);
                SetupRelease();

                await Subject.ReleaseInstanceAsync(ctxt, _instance, new Exception());

                Assert.True((_instance as InstanceObject).Disposed);
            }

            [Fact]
            public async Task ReleaseWhenError_Action_InstanceNotDisposed()
            {
                var ctxt = CreateContext(Contract.Action);
                SetupRelease();

                await Subject.ReleaseInstanceAsync(ctxt, _instance, new Exception());

                Assert.False((_instance as InstanceObject).Disposed);
            }

            [Fact]
            public async Task ReleaseWhenError_Destroy_InstanceDisposed()
            {
                var ctxt = CreateContext(Contract.Destroy);
                SetupRelease();

                await Subject.ReleaseInstanceAsync(ctxt, _instance, new Exception());

                Assert.True((_instance as InstanceObject).Disposed);
            }

            private void SetupRelease(ServerActionContext ctxt = null)
            {
                Mock.Setup(o => o.OnInstanceReleased(ctxt ?? _context, SessionHeaderValue)).Verifiable();
            }

            protected override ServerActionContext CreateContext(ActionDescriptor descriptor)
            {
                var ctxt = base.CreateContext(descriptor);
                ctxt.HttpContext.Request.Headers[SessionHeader] = SessionHeaderValue;
                return ctxt;
            }

            private class InstanceObject : IDisposable
            {
                public bool Disposed { get; set; }

                public void Dispose()
                {
                    Disposed = true;
                }
            }
        }
    }

    public abstract class StatefullinstanceProviderTestBase
    {
        protected const string SessionHeader = "test-session-id";

        public StatefullinstanceProviderTestBase()
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

            string CreateNewSession();

            void OnInstanceCreated(ServerActionContext context, string sessionId);

            void OnInstanceReleased(ServerActionContext context, string sessionId);
        }

        protected class MockStateFullInstanceProvider : StateFullInstanceProvider
        {
            private readonly Mock<IInstanceProviderActions> _actions;

            public MockStateFullInstanceProvider(MockContractDescriptor contract, string header, Mock<IInstanceProviderActions> actions) : base(contract.Init, contract.Destroy, new BoltServerOptions() { SessionHeader = header })
            {
                _actions = actions;
            }

            protected override object CreateInstance(ServerActionContext context, Type type)
            {
                return _actions.Object.CreateInstance(context, type);
            }

            protected override string CreateNewSession()
            {
                return _actions.Object.CreateNewSession();
            }

            protected override async Task OnInstanceCreatedAsync(ServerActionContext context, string sessionId)
            {
                _actions.Object.OnInstanceCreated(context, sessionId);
            }

            protected override async Task OnInstanceReleasedAsync(ServerActionContext context, string sessionId)
            {
                _actions.Object.OnInstanceReleased(context, sessionId);
            }
        }

        protected virtual ServerActionContext CreateContext(ActionDescriptor descriptor)
        {
            return new ServerActionContext()
            {
                Action = descriptor,
                HttpContext = new DefaultHttpContext()
            };
        }

        protected virtual MockStateFullInstanceProvider CreateSubject()
        {
            return new MockStateFullInstanceProvider(Contract, SessionHeader, Mock);
        }

        protected MockContractDescriptor Contract { get; private set; }
    }
}
