using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Localization;
using Moq;
using Xunit;

namespace Bolt.Server.IntegrationTest
{
    public class AcceptLanguageFilterTest : FiltersTestBase
    {
        [Fact(Skip = "Not working... investigate ")]
        public void SendCulture_EnsureProperCultureOnServer()
        {
            CultureInfo expectedCulture = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures).Except(new[]{ CultureInfo.CurrentCulture}).Last();

            Thread.CurrentThread.CurrentCulture = expectedCulture;

            Callback = new Mock<IFiltersContract>();
            Callback.Setup(f => f.OnExecute()).Callback(() =>
            {
                Assert.Equal(expectedCulture.Name, CultureInfo.CurrentCulture.Name);
            }).Verifiable();

            CreateChannel().OnExecute();

            Callback.Verify();
        }

        [Fact(Skip = "Not working... investigate ")]
        public void SendCulture_EnsureProperUICultureOnServer()
        {
            CultureInfo expectedCulture = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures).Except(new[] { CultureInfo.CurrentCulture }).Last();

            Thread.CurrentThread.CurrentCulture = expectedCulture;

            Callback = new Mock<IFiltersContract>();
            Callback.Setup(f => f.OnExecute()).Callback(() =>
            {
                Assert.Equal(expectedCulture.Name, CultureInfo.CurrentUICulture.Name);
            }).Verifiable();

            CreateChannel().OnExecute();

            Callback.Verify();
        }

        protected override void Configure(IApplicationBuilder appBuilder)
        {
            RequestLocalizationOptions options = new RequestLocalizationOptions();
            options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
            appBuilder.UseRequestLocalization(options, new RequestCulture("en-us"));
            base.Configure(appBuilder);
        }

        protected override IFiltersContract CreateChannel()
        {
            return ClientConfiguration.ProxyBuilder().Url(ServerUrl).PreserveCultureInfo().Build<IFiltersContract>();
        }
    }
}