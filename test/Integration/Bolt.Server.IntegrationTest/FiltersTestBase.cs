using Bolt.Client;

using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;

using Moq;

namespace Bolt.Server.IntegrationTest
{
    public class FiltersTestBase : IntegrationTestBase, ITestContext
    {
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

        protected internal Mock<IFiltersContract> Callback { get; set; }

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

        public object Instance => this;
    }
}