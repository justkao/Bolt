using System;
using System.Reflection;
using System.Threading.Tasks;
using Bolt.Server.InstanceProviders;
using Moq;
using Xunit;

namespace Bolt.Server.Test
{
    public class StatefullinstanceProviderTests
    {
        public class ReleaseInstanceWithoutSession : StatefullinstanceProviderTestBase
        {
            [Fact]
            public async Task Destroy_ShouldNotThrow()
            {
                var ctxt = CreateContext(BoltFramework.DestroySessionAction);

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
                var ctxt = CreateContext(BoltFramework.InitSessionAction);
                SetupInit(ctxt);

                await Subject.GetInstanceAsync(ctxt, typeof(IMockContract));

                Mock.Verify();
            }

            [Fact]
            public async Task Init_EnsureInstanceCreatedAndCached()
            {
                var ctxt = CreateContext(BoltFramework.InitSessionAction);
                SetupInit(ctxt);

                await Subject.GetInstanceAsync(ctxt, typeof(IMockContract));

                Assert.Equal(1, Subject.LocalCount);
            }

            [Fact]
            public async Task Init_EnsureSessionHeaderCreated()
            {
                var ctxt = CreateContext(BoltFramework.InitSessionAction);
                SetupInit(ctxt);

                await Subject.GetInstanceAsync(ctxt, typeof(IMockContract));

                Assert.NotNull(ctxt.HttpContext.Response.Headers[SessionHeader]);
            }

            [Fact]
            public async Task Init_EnsureSessionHeaderValue()
            {
                var ctxt = CreateContext(BoltFramework.InitSessionAction);
                SetupInit(ctxt, "testsession");

                await Subject.GetInstanceAsync(ctxt, typeof(IMockContract));

                Assert.Equal("testsession", ctxt.HttpContext.Response.Headers[SessionHeader]);
            }

            [Fact]
            public async Task Init_EnsureInstance()
            {
                var ctxt = CreateContext(BoltFramework.InitSessionAction);
                var instance = new object();

                SetupInit(ctxt, "testsession", instance);

                var result = await Subject.GetInstanceAsync(ctxt, typeof(IMockContract));

                Assert.Equal(instance, result);
            }

            [Fact]
            public async Task Init_EnsureOnInstanceCreatedCalled()
            {
                var ctxt = CreateContext(BoltFramework.InitSessionAction);
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
                var ctxt = CreateContext(BoltFramework.DestroySessionAction);
                await Assert.ThrowsAsync<SessionHeaderNotFoundException>(() => Subject.GetInstanceAsync(ctxt, typeof(IMockContract)));
            }

            private void SetupInit(ServerActionContext ctxt, string session = "testsession", object instance = null)
            {
                Mock.Setup(o => o.CreateInstance(ctxt, typeof(IMockContract))).Returns(instance ?? new object()).Verifiable();
                Mock.Setup(o => o.GenerateSessionid()).Returns(session ?? Guid.NewGuid().ToString()).Verifiable();

            }
        }

        public class GetInstanceWithExistingSession : StatefullinstanceProviderTestBase
        {
            private string _sessionHeaderValue = "session value";

            private readonly object _instance;

            public GetInstanceWithExistingSession()
            {
                var ctxt = CreateContext(BoltFramework.InitSessionAction);
                _instance = new object();

                Mock.Setup(o => o.CreateInstance(It.IsAny<ServerActionContext>(), typeof(IMockContract))).Returns(_instance).Verifiable();
                Mock.Setup(o => o.GenerateSessionid()).Returns(_sessionHeaderValue);

                Subject.GetInstanceAsync(ctxt, typeof(IMockContract)).GetAwaiter().GetResult();
            }

            [Fact]
            public async Task Init_ExistingSession_Ok()
            {
                var ctxt = CreateContext(BoltFramework.InitSessionAction);

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
                _sessionHeaderValue = "another session";
                var ctxt = CreateContext(Contract.Action);

                await Assert.ThrowsAsync<SessionNotFoundException>(() => Subject.GetInstanceAsync(ctxt, typeof(IMockContract)));
            }

            [Fact]
            public async Task Destroy_DifferentSession_EnsureThrows()
            {
                _sessionHeaderValue = "another session";
                var ctxt = CreateContext(BoltFramework.DestroySessionAction);

                await Assert.ThrowsAsync<SessionNotFoundException>(() => Subject.GetInstanceAsync(ctxt, typeof(IMockContract)));
            }

            protected override ServerActionContext CreateContext(MethodInfo action)
            {
                var ctxt = base.CreateContext(action);
                ctxt.HttpContext.Request.Headers[SessionHeader] = _sessionHeaderValue;
                return ctxt;
            }
        }

        public class ReleaseInstanceWithExistingSession : StatefullinstanceProviderTestBase
        {
            private string SessionHeaderValue = "session value";

            private readonly object _instance;

            private readonly ServerActionContext _context;

            private IContractSession _session;

            public ReleaseInstanceWithExistingSession()
            {
                _context = CreateContext(BoltFramework.InitSessionAction);
                _instance = new InstanceObject();

                Mock.Setup(o => o.CreateInstance(It.IsAny<ServerActionContext>(), typeof(IMockContract))).Returns(_instance).Verifiable();
                Mock.Setup(o => o.GenerateSessionid()).Returns(SessionHeaderValue);

                Subject.GetInstanceAsync(_context, typeof(IMockContract)).GetAwaiter().GetResult();
                _session = _context.HttpContext.GetFeature<IContractSession>();
            }

            [Fact]
            public async Task Release_DestroyAction_Ok()
            {
                var ctxt = CreateContext(BoltFramework.DestroySessionAction);

                SetupRelease(ctxt);

                await Subject.ReleaseInstanceAsync(ctxt, _instance, null);

                Mock.Verify();
            }

            [Fact]
            public async Task  Release_DestroyAction_InstanceDestroyed()
            {
                var ctxt = CreateContext(BoltFramework.DestroySessionAction);

                SetupRelease();

                await Subject.ReleaseInstanceAsync(ctxt, _instance, null);

                Assert.Equal(0, Subject.LocalCount);
            }

            [Fact]
            public async Task Release_DestroyAction_OnInstanceReleasedCalled()
            {
                var ctxt = CreateContext(BoltFramework.DestroySessionAction);

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
                var ctxt = CreateContext(BoltFramework.DestroySessionAction);
                SetupRelease();

                await Subject.ReleaseInstanceAsync(ctxt, _instance, null);

                Assert.True(((InstanceObject) _instance).Disposed);
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
                var ctxt = CreateContext(BoltFramework.InitSessionAction);
                SetupRelease();

                await Subject.ReleaseInstanceAsync(ctxt, _instance, new Exception());

                Assert.True(((InstanceObject) _instance).Disposed);
            }

            [Fact]
            public async Task ReleaseWhenError_Action_InstanceNotDisposed()
            {
                var ctxt = CreateContext(Contract.Action);
                SetupRelease();

                await Subject.ReleaseInstanceAsync(ctxt, _instance, new Exception());

                Assert.False(((InstanceObject) _instance).Disposed);
            }

            [Fact]
            public async Task ReleaseWhenError_Destroy_InstanceDisposed()
            {
                var ctxt = CreateContext(BoltFramework.DestroySessionAction);
                SetupRelease();

                await Subject.ReleaseInstanceAsync(ctxt, _instance, new Exception());

                Assert.True(((InstanceObject) _instance).Disposed);
            }

            private void SetupRelease(ServerActionContext ctxt = null)
            {
                Mock.Setup(o => o.OnInstanceReleased(ctxt ?? _context, SessionHeaderValue)).Verifiable();
            }

            protected override ServerActionContext CreateContext(MethodInfo action)
            {
                var ctxt = base.CreateContext(action);
                ctxt.HttpContext.SetFeature(_session);
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
}