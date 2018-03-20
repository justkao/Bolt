using Bolt.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Bolt.Server.IntegrationTest
{
    public class FiltersTestBase : IntegrationTestBase, ITestContext
    {
        public object Instance => this;

        protected internal Mock<IFiltersContract> Callback { get; set; }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITestContext>(c => this);
            base.ConfigureServices(services);
        }

        protected override void Configure(IApplicationBuilder appBuilder)
        {
            appBuilder.UseBolt(h =>
            {
                h.Use<IFiltersContract, FiltersContract>();
            });
        }

        protected virtual IFiltersContract CreateChannel()
        {
            return ClientConfiguration.CreateProxy<IFiltersContract>(ServerUrl);
        }

        public interface IFiltersContract
        {
            void OnExecute();
        }

        public class FiltersContract : IFiltersContract
        {
            private readonly ITestContext _context;

            public FiltersContract(ITestContext context)
            {
                _context = context;
            }

            public void OnExecute()
            {
                ((FiltersTestBase)_context.Instance).Callback?.Object.OnExecute();
            }
        }
    }
}