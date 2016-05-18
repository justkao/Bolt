using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bolt.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class DistributedSessionTest : IntegrationTestBase, ITestContext
    {
        [Fact]
        public void Execute_EnsureHasSession()
        {
            Callback = new Mock<IDummyContract>();

            DistributedCache.Setup(c => c.RefreshAsync(It.IsAny<string>())).Returns(Task.FromResult(true)).Verifiable();

            Callback.Setup(c => c.OnExecute(It.IsAny<object>())).Callback<object>(
                v =>
                    {
                        Assert.NotNull(((DummyContract)v).HttpSessionProvider.Session);
                        Assert.NotNull(((DummyContract)v).HttpSessionProvider.SessionId);
                    }).Verifiable();

            CreateChannel().OnExecute(null);

            Callback.Verify();
            DistributedCache.Verify();
        }

        [Fact]
        public void Execute_MutltipleTimes_EnsureSameSession()
        {
            Callback = new Mock<IDummyContract>();

            string sessionId = null;

            DistributedCache.Setup(c => c.RefreshAsync(It.IsAny<string>())).Returns(Task.FromResult(true)).Verifiable();
            Callback.Setup(c => c.OnExecute(It.IsAny<object>())).Callback<object>(
                v =>
                {
                    if (sessionId == null)
                    {
                        sessionId = ((DummyContract)v).HttpSessionProvider.SessionId;
                    }
                    else
                    {
                        Assert.Equal(sessionId, ((DummyContract)v).HttpSessionProvider.SessionId);
                    }
                }).Verifiable();

            var channel = CreateChannel();
            channel.OnExecute(null);
            channel.OnExecute(null);

            Callback.Verify();
            DistributedCache.Verify();
        }

        [Fact]
        public void MultipleClients_EnsureUniqueSessions()
        {
            Callback = new Mock<IDummyContract>();
            List<string> sessions = new List<string>();

            DistributedCache.Setup(c => c.RefreshAsync(It.IsAny<string>())).Returns(Task.FromResult(true)).Verifiable();
            Callback.Setup(c => c.OnExecute(It.IsAny<object>())).Callback<object>(
                v =>
                {
                    sessions.Add(((DummyContract)v).HttpSessionProvider.SessionId);
                }).Verifiable();

            for (int i = 0; i < 10; i++)
            {
                CreateChannel().OnExecute(null);
            }

            Assert.True(sessions.Distinct().Count() == 10, "Unique sessions were not created");
        }

        [Fact]
        public void AccessSession_NotNull()
        {
            Callback = new Mock<IDummyContract>();

            DistributedCache.Setup(c => c.RefreshAsync(It.IsAny<string>())).Returns(Task.FromResult(true)).Verifiable();

            Callback.Setup(c => c.OnExecute(It.IsAny<object>())).Callback<object>(
                v =>
                    {
                        ISession session = ((DummyContract)v).HttpSessionProvider.Session;
                        Assert.NotNull(session);
                    }).Verifiable();

            CreateChannel().OnExecute(null);
        }

        [Fact]
        public void LoadSession_Ok()
        {
            Callback = new Mock<IDummyContract>();

            DistributedCache.Setup(c => c.RefreshAsync(It.IsAny<string>())).Returns(Task.FromResult(true)).Verifiable();
            DistributedCache.Setup(c => c.GetAsync(It.IsAny<string>())).Returns(Task.FromResult((byte[])null)).Verifiable();

            Callback.Setup(c => c.OnExecute(It.IsAny<object>())).Callback<object>(
                v =>
                {
                    ISession session = ((DummyContract)v).HttpSessionProvider.Session;
                    session.LoadAsync().GetAwaiter().GetResult();
                }).Verifiable();

            CreateChannel().OnExecute(null);
        }

        [Fact]
        public void LoadSession_GetValue_Ok()
        {
            Callback = new Mock<IDummyContract>();

            DistributedCache.Setup(c => c.RefreshAsync(It.IsAny<string>())).Returns(Task.FromResult(true)).Verifiable();
            DistributedCache.Setup(c => c.Get(It.IsAny<string>())).Returns(((byte[])null)).Verifiable();

            Callback.Setup(c => c.OnExecute(It.IsAny<object>())).Callback<object>(
                v =>
                {
                    ISession session = ((DummyContract)v).HttpSessionProvider.Session;
                    session.Get("temp");
                }).Verifiable();

            CreateChannel().OnExecute(null);
        }

        [Fact]
        public void LoadSession_SetValue_Ok()
        {
            Callback = new Mock<IDummyContract>();
            DistributedCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>()))
                .Returns(Task.FromResult(true))
                .Verifiable();
            DistributedCache.Setup(c => c.GetAsync(It.IsAny<string>())).Returns(Task.FromResult((byte[])null)).Verifiable();
            DistributedCache.Setup(c => c.Get(It.IsAny<string>())).Returns(((byte[])null)).Verifiable();

            Callback.Setup(c => c.OnExecute(It.IsAny<object>())).Callback<object>(
                v =>
                {
                    ISession session = ((DummyContract)v).HttpSessionProvider.Session;
                    session.Set("temp", new byte[10]);
                }).Verifiable();

            CreateChannel().OnExecute(null);

            DistributedCache.Verify();
        }

        [Fact]
        public void GetSession_EnsureSameDistributedSession()
        {
            List<string> sessions = new List<string>();
                 
            Callback = new Mock<IDummyContract>();
            DistributedCache.Setup(c => c.RefreshAsync(It.IsAny<string>())).Returns(Task.FromResult(true)).Callback<string>(
                v =>
                    {
                        sessions.Add(v);
                    }).Verifiable();

            Callback.Setup(c => c.OnExecute(It.IsAny<object>())).Callback<object>(
                v =>
                    {
                        sessions.Add(((DummyContract)v).HttpSessionProvider.SessionId);
                    }).Verifiable();

            IDummyContract channel = CreateChannel();
            channel.OnExecute(null);
            channel.OnExecute(null);
            channel.OnExecute(null);

            Assert.True(sessions.Distinct().Count()== 1, "Multiple sessions were created.");
            DistributedCache.Verify();
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddSingleton<ITestContext>(c => this);
            base.ConfigureServices(services);
        }

        protected override void Configure(IApplicationBuilder appBuilder)
        {
            DistributedCache = new Mock<IDistributedCache>(MockBehavior.Strict);
            DistributedSessionStore store = new DistributedSessionStore(
                DistributedCache.Object,
                appBuilder.ApplicationServices.GetRequiredService<ILoggerFactory>());

            appBuilder.UseBolt(
                h =>
                    {
                        h.UseDistributedSession<IDummyContract, DummyContract>(store);
                    });
        }

        protected internal Mock<IDummyContract> Callback { get; set; }

        protected internal Mock<IDistributedCache> DistributedCache { get; set; }

        protected virtual IDummyContract CreateChannel()
        {
            return ClientConfiguration.CreateSessionProxy<IDummyContract>(ServerUrl);
        }

        public class DummyContract : IDummyContract
        {
            public DummyContract(ITestContext context, IHttpSessionProvider httpSessionProvider)
            {
                HttpSessionProvider = httpSessionProvider;
                _context = context;
            }

            private readonly ITestContext _context;

            public IHttpSessionProvider HttpSessionProvider { get; }

            public void OnExecute(object context)
            {
                ((DistributedSessionTest)_context.Instance).Callback?.Object.OnExecute(this);
            }
        }

        public object Instance => this;
    }
}