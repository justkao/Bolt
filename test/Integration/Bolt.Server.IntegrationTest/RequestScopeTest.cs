using System;
using System.Threading;
using System.Threading.Tasks;

using Bolt.Client;
using Microsoft.AspNet.Builder;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class RequestScopeTest : IntegrationTestBase, ITestContext
    {
        [Fact]
        public void Execute_WithTimeout_EnsureTimeoutException()
        {
            Callback = new Mock<IDummyContract>();
            Callback.Setup(c => c.OnExecute(It.IsAny<object>())).Callback<object>(
                v =>
                    {
                        Task.Delay(TimeSpan.FromSeconds(4)).GetAwaiter().GetResult();
                    }).Verifiable();

            using (new RequestScope(TimeSpan.FromSeconds(1)))
            {
                Assert.Throws<TimeoutException>(() => CreateChannel().OnExecute(null));
            }

            Callback.Verify();
        }

        [Fact(Skip = "Unstable... investigate ")]
        public void Execute_WithCancellation_EnsureOperationCancelledException()
        {
            CancellationTokenSource source = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            Callback = new Mock<IDummyContract>();
            Callback.Setup(c => c.OnExecute(It.IsAny<object>())).Callback<object>(
                v =>
                {
                    Task.Delay(TimeSpan.FromSeconds(4)).GetAwaiter().GetResult();
                }).Verifiable();

            using (new RequestScope(cancellation:source.Token))
            {
                Assert.Throws<OperationCanceledException>(() => CreateChannel().OnExecute(null));
            }

            Callback.Verify();
        }


        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddSingleton<ITestContext>(c => this);
            base.ConfigureServices(services);
        }

        protected override void Configure(IApplicationBuilder appBuilder)
        {
            appBuilder.UseBolt(
                h =>
                    {
                        h.Use<IDummyContract, DummyContract>();
                    });
        }

        protected internal Mock<IDummyContract> Callback { get; set; }

        protected virtual IDummyContract CreateChannel()
        {
            return ClientConfiguration.CreateProxy<IDummyContract>(ServerUrl);
        }

        public class DummyContract : IDummyContract
        {
            public DummyContract(ITestContext context)
            {
                _context = context;
            }

            private readonly ITestContext _context;

            public void OnExecute(object context)
            {
                ((RequestScopeTest)_context.Instance).Callback.Object.OnExecute(this);
            }
        }

        public object Instance => this;
    }
}