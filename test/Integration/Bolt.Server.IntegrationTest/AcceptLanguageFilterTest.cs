using System.Globalization;
using System.Linq;
using System.Threading;
using Bolt.Client.Proxy;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Localization;
using Moq;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class AcceptLanguageFilterTest : FiltersTestBase
    {
        public AcceptLanguageFilterTest()
        {
            ClientConfiguration.UseDynamicProxy();
        }

        [Fact]
        public void SendCulture_EnsureProperCultureOnServer()
        {
            CultureInfo expectedCulture = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures).Except(new[]{ CultureInfo.CurrentCulture}).Last();

            Thread.CurrentThread.CurrentCulture = expectedCulture;

            Callback = new Mock<IFiltersContract>();
            Callback.Setup(f => f.OnExecute()).Callback(() =>
            {
                Assert.Equal(expectedCulture, CultureInfo.CurrentCulture);
            }).Verifiable();

            CreateChannel().OnExecute();

            Callback.Verify();
        }

        [Fact]
        public void SendCulture_EnsureProperUICultureOnServer()
        {
            CultureInfo expectedCulture = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures).Except(new[] { CultureInfo.CurrentCulture }).Last();

            Thread.CurrentThread.CurrentCulture = expectedCulture;

            Callback = new Mock<IFiltersContract>();
            Callback.Setup(f => f.OnExecute()).Callback(() =>
            {
                Assert.Equal(expectedCulture, CultureInfo.CurrentUICulture);
            }).Verifiable();

            CreateChannel().OnExecute();

            Callback.Verify();
        }

        protected override void Configure(IApplicationBuilder appBuilder)
        {
            appBuilder.UseRequestLocalization(new RequestCulture("en-us"));
            base.Configure(appBuilder);
        }

        protected override IFiltersContract CreateChannel()
        {
            return ClientConfiguration.ProxyBuilder().Url(ServerUrl).PreserveCultureInfo().Build<IFiltersContract>();
        }
    }
}